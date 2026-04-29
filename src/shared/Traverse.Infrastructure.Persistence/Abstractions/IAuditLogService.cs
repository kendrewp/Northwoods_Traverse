namespace Traverse.Infrastructure.Persistence.Abstractions;

/// <summary>
/// Records audit log entries for entity mutations.
/// </summary>
/// <remarks>
/// Exposed as an interface (rather than calling a concrete service directly)
/// so application code remains testable without a real database — handlers
/// inject this interface, infrastructure provides the implementation.
/// </remarks>
public interface IAuditLogService
{
    /// <summary>
    /// Writes a single audit entry.
    /// </summary>
    /// <param name="entityType">CLR or domain type name of the entity (e.g., "Order").</param>
    /// <param name="entityId">String form of the entity identity.</param>
    /// <param name="action">Verb describing the change (typically "Insert", "Update", "Delete").</param>
    /// <param name="changedBy">Identifier of the actor responsible.</param>
    /// <param name="oldValues">Pre-change snapshot (any object — JSON-serialised by the implementation), or <c>null</c> for inserts.</param>
    /// <param name="newValues">Post-change snapshot, or <c>null</c> for deletes.</param>
    /// <param name="ct">Cancellation token.</param>
    Task LogAsync(
        string entityType,
        string entityId,
        string action,
        string changedBy,
        object? oldValues,
        object? newValues,
        CancellationToken ct);
}
