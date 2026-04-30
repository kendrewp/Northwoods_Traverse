using Traverse.Infrastructure.Messaging.Entities;

namespace Traverse.Infrastructure.Messaging.Processing;

/// <summary>
/// Abstract base for outbox background processors.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Template Method pattern (OCP):</strong> this base owns the scheduling
/// loop — poll every 5 seconds, create a DI scope, obtain a publisher — and
/// delegates the actual database query and publish logic to the subclass via
/// <see cref="ProcessScopeAsync"/>. Adding support for a new message type or a
/// new service's <c>DbContext</c> never requires modifying this class.
/// </para>
/// <para>
/// <strong>Why scope-per-iteration:</strong> <see cref="BackgroundService"/> is
/// registered as a Singleton (the host owns its lifetime). EF Core's
/// <c>DbContext</c> is Scoped — a Singleton must never hold a Scoped service
/// directly, or it captures a disposed context on subsequent iterations. By
/// creating a new <see cref="IServiceScope"/> at the top of each iteration, we
/// resolve a fresh <c>DbContext</c> whose lifetime is bounded to that batch of
/// work. The scope is disposed before the delay, releasing the connection back
/// to the pool promptly.
/// </para>
/// <para>
/// <strong>Why the abstract method receives <c>IServiceProvider</c>:</strong>
/// the concrete <c>DbContext</c> type is unknown at the base level. Rather than
/// parameterise the base with a generic type argument (which would complicate
/// DI registration), we pass the scoped <see cref="IServiceProvider"/> and let
/// the subclass call <c>GetRequiredService&lt;MyDbContext&gt;()</c>. The subclass
/// also receives the scoped <see cref="IPublishEndpoint"/> so it does not have
/// to resolve it a second time.
/// </para>
/// </remarks>
public abstract class OutboxProcessorBase : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initialises the processor with the root <see cref="IServiceProvider"/>
    /// used to create per-iteration DI scopes.
    /// </summary>
    /// <param name="serviceProvider">Root DI container. Must not be null.</param>
    protected OutboxProcessorBase(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Runs until <paramref name="stoppingToken"/> is cancelled. Each iteration
    /// processes one batch of unpublished outbox rows, then waits 5 seconds.
    /// The delay is intentionally short so events reach the broker within
    /// seconds of the producing transaction committing.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessBatchAsync(stoppingToken).ConfigureAwait(false);

            // 5-second poll interval — balances broker latency against DB load.
            // If stoppingToken fires during the delay, Task.Delay throws
            // OperationCanceledException which BackgroundService catches gracefully.
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Creates a DI scope, resolves a <see cref="IPublishEndpoint"/>, and
    /// delegates to <see cref="ProcessScopeAsync"/> with the scoped provider.
    /// </summary>
    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        // Create a new scope for this iteration. Disposing the scope returns
        // the DbContext's DB connection to the pool and releases all scoped
        // services, preventing connection leaks between poll intervals.
        using var scope = _serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await ProcessScopeAsync(scope.ServiceProvider, publisher, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Performs the actual outbox query, publish, and save-changes within the
    /// provided DI scope.
    /// </summary>
    /// <param name="scopedProvider">
    /// Scoped <see cref="IServiceProvider"/> from which the subclass resolves
    /// its concrete <c>DbContext</c> (e.g.,
    /// <c>scopedProvider.GetRequiredService&lt;MyDbContext&gt;()</c>).
    /// </param>
    /// <param name="publisher">
    /// Scoped MassTransit publisher already resolved from the same scope — pass
    /// directly to <c>IPublishEndpoint.Publish</c> to avoid double-resolve.
    /// </param>
    /// <param name="ct">Cancellation token propagated from the host shutdown.</param>
    /// <returns>
    /// A <see cref="Task"/> that completes when the batch is processed and
    /// changes are saved to the database.
    /// </returns>
    /// <example>
    /// Typical subclass implementation:
    /// <code>
    /// protected override async Task ProcessScopeAsync(
    ///     IServiceProvider scopedProvider,
    ///     IPublishEndpoint publisher,
    ///     CancellationToken ct)
    /// {
    ///     var db = scopedProvider.GetRequiredService&lt;MyServiceDbContext&gt;();
    ///     var messages = await db.OutboxMessages
    ///         .Where(m => m.PublishedOnUtc == null)
    ///         .OrderBy(m => m.CreatedOnUtc)
    ///         .Take(100)
    ///         .ToListAsync(ct);
    ///
    ///     foreach (var message in messages)
    ///     {
    ///         var payload = JsonSerializer.Deserialize(message.Content,
    ///             Type.GetType(message.MessageType)!)!;
    ///         await publisher.Publish(payload, ct);
    ///         message.PublishedOnUtc = DateTime.UtcNow;
    ///     }
    ///
    ///     await db.SaveChangesAsync(ct);
    /// }
    /// </code>
    /// </example>
    protected abstract Task ProcessScopeAsync(
        IServiceProvider scopedProvider,
        IPublishEndpoint publisher,
        CancellationToken ct);
}
