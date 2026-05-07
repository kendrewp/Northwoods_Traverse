using MediatR;
using Traverse.Infrastructure.Persistence.Entities;

namespace Traverse.Infrastructure.Persistence.Context;

/// <summary>
/// Abstract base <see cref="DbContext"/> for every Traverse service.
/// </summary>
/// <remarks>
/// <para>
/// The name <c>TraverseDbContext</c> (rather than <c>ApplicationDbContext</c>)
/// is intentional and authoritative — see audit finding AF-001. Each service
/// derives a concrete context (e.g., <c>WorkflowDbContext</c>) from this base
/// and adds its own <see cref="DbSet{TEntity}"/> properties.
/// </para>
/// <para>
/// <strong>Domain event dispatch (post-commit):</strong> the override of
/// <see cref="SaveChangesAsync"/> dispatches events <em>after</em>
/// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> commits the
/// underlying transaction. This guarantees that subscribers only react to
/// committed facts. <strong>Trade-off:</strong> if
/// <see cref="IPublisher.Publish{TNotification}"/> throws after commit, the
/// data is already persisted but the event is lost (no retry). Cross-service
/// events should therefore travel through the outbox pattern; in-process
/// dispatch via MediatR is reserved for synchronous side-effects within the
/// same request boundary.
/// </para>
/// <para>
/// <strong>Configuration discovery:</strong> <see cref="OnModelCreating"/>
/// calls <c>ApplyConfigurationsFromAssembly(GetType().Assembly)</c>. Using
/// <c>GetType()</c> resolves to the <em>concrete</em> service assembly at
/// runtime, which is what we want — the base assembly only carries
/// <see cref="Configurations.AuditLogConfiguration"/>, while service-specific
/// configurations live in the service's own assembly.
/// </para>
/// </remarks>
public abstract class TraverseDbContext : DbContext
{
    private readonly IPublisher _mediator;

    /// <summary>
    /// Constructs the context with EF Core options and a MediatR publisher used
    /// for post-commit domain event dispatch.
    /// </summary>
    /// <param name="options">EF Core options supplied by DI.</param>
    /// <param name="mediator">MediatR publisher used to dispatch domain events. Required.</param>
    protected TraverseDbContext(DbContextOptions options, IPublisher mediator)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(mediator);
        _mediator = mediator;
    }

    /// <summary>Audit log table — present in every service's database.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        // Discover entity configurations in the concrete service assembly (the
        // runtime type), not in this base assembly. This is the standard
        // pattern for multi-assembly EF Core configuration discovery.
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // Also register the configurations defined in this base assembly
        // (only AuditLogConfiguration in Phase 0). Without this call,
        // services that derive from us would have to remember to register
        // AuditLogConfiguration themselves — an avoidable foot-gun.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TraverseDbContext).Assembly);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Snapshot the events BEFORE saving. The aggregates are still tracked
        // and we want to capture the full set even if SaveChanges adds further
        // tracking changes.
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // Commit the underlying transaction first. Any failure here propagates
        // to the caller before we publish anything — so failed transactions
        // never produce phantom events.
        var result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Clear domain events on every aggregate so a subsequent save does not
        // republish them. This must happen regardless of dispatch outcome — if
        // we cleared after publish, an exception mid-loop would leave some
        // aggregates with un-cleared events that would re-fire on the next save.
        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // Dispatch events. If a handler throws, the data is already committed —
        // the event is lost. Callers that need at-least-once semantics for
        // cross-service events MUST use the outbox pattern instead of
        // in-process MediatR dispatch.
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, cancellationToken).ConfigureAwait(false);
        }

        return result;
    }
}
