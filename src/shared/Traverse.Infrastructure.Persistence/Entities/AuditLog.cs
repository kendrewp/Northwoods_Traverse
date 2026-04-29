namespace Traverse.Infrastructure.Persistence.Entities;

/// <summary>
/// Append-only audit trail entry. One row per change event for any auditable
/// entity in any service.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="OldValues"/> and <see cref="NewValues"/> are JSON snapshots
/// (untyped) so the audit table stays generic across all entity types. Storing
/// JSON keeps the audit log self-describing without requiring per-entity
/// schema changes when business entities evolve.
/// </para>
/// <para>
/// The composite index <c>(EntityType, EntityId)</c> is configured in
/// <see cref="Configurations.AuditLogConfiguration"/>; it serves the most
/// common query pattern: "show all audit entries for entity X of type Y".
/// </para>
/// </remarks>
public class AuditLog
{
    /// <summary>Primary key — generated client-side so audit entries can be assigned an ID before insertion.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>CLR or domain type name of the audited entity (e.g., "Order"). Max 256.</summary>
    public required string EntityType { get; init; }

    /// <summary>String form of the audited entity's identity. Max 128.</summary>
    public required string EntityId { get; init; }

    /// <summary>Action performed: "Insert", "Update", "Delete", or service-specific verb. Max 50.</summary>
    public required string Action { get; init; }

    /// <summary>Identifier of the actor responsible for the change. Max 256.</summary>
    public required string ChangedBy { get; init; }

    /// <summary>UTC timestamp of the change.</summary>
    public DateTimeOffset ChangedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>JSON snapshot of the entity state before the change, or <c>null</c> for inserts.</summary>
    public string? OldValues { get; init; }

    /// <summary>JSON snapshot of the entity state after the change, or <c>null</c> for deletes.</summary>
    public string? NewValues { get; init; }
}
