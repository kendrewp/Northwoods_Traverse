namespace Traverse.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Unit-of-work abstraction over the underlying <see cref="DbContext"/>.
/// </summary>
/// <remarks>
/// Exposes only <see cref="SaveChangesAsync"/> so consumers — typically
/// command handlers — cannot accidentally take a dependency on the full EF
/// Core surface (Interface Segregation Principle). Each service registers a
/// concrete implementation that delegates to its own <c>DbContext</c>.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all tracked changes in the current logical transaction.
    /// </summary>
    /// <param name="ct">Cancellation token honoured by the underlying DbContext.</param>
    /// <returns>Number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct);
}
