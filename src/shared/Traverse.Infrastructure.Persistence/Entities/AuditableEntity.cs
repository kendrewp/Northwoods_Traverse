namespace Traverse.Infrastructure.Persistence.Entities;

/// <summary>
/// Mix-in base class supplying created/modified audit columns.
/// </summary>
/// <remarks>
/// <para>
/// This is <em>not</em> an EF Core entity in its own right — there is no
/// <c>AuditableEntities</c> table. Concrete service entities inherit from this
/// type so EF Core picks up the columns via shadow-state composition. The
/// 256-character maximum length is documented here and enforced via
/// <c>HasMaxLength(256)</c> in each service's per-entity configuration.
/// </para>
/// <para>
/// <see cref="CreatedAt"/> and <see cref="CreatedBy"/> are <c>init</c>-only —
/// they are set by application code at insertion time and must never change
/// afterwards. <see cref="ModifiedAt"/> and <see cref="ModifiedBy"/> are
/// nullable and mutable because update timestamps are written on every save.
/// </para>
/// </remarks>
public abstract class AuditableEntity
{
    /// <summary>UTC timestamp the entity was created. Defaults to construction time.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Identifier (typically email or sub claim) of the actor who created the
    /// entity. Required at construction; max length 256 enforced by service config.
    /// </summary>
    public required string CreatedBy { get; init; }

    /// <summary>UTC timestamp of the most recent modification, or <c>null</c> if never modified.</summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Identifier of the actor who performed the most recent modification, or
    /// <c>null</c> if never modified. Max length 256 enforced by service config.
    /// </summary>
    public string? ModifiedBy { get; set; }
}
