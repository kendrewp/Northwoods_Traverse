using Traverse.Infrastructure.Persistence.Entities;

namespace Traverse.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="AuditLog"/> entity.
/// </summary>
/// <remarks>
/// All length constraints come from the design document § 4.2 and are enforced
/// at the database layer (not relied upon at the C# layer) so that bad data
/// fails the insert rather than passing into a downstream system. The
/// composite index on <c>(EntityType, EntityId)</c> serves the dominant
/// audit-trail query pattern: "give me everything that ever happened to this
/// entity".
/// </remarks>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ChangedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(a => a.ChangedAt)
            .IsRequired();

        // OldValues / NewValues stay unbounded — JSON snapshots can be large
        // and truncating them would silently corrupt the audit record.
        builder.Property(a => a.OldValues);
        builder.Property(a => a.NewValues);

        // Composite index supports the "audit trail for entity X" query.
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
    }
}
