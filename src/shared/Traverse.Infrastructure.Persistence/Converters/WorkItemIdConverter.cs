using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter for <see cref="WorkItemId"/>. See
/// <see cref="ClientIdConverter"/> for the rationale behind using
/// <see cref="WorkItemId.From(Guid)"/> rather than the raw constructor.
/// </summary>
public sealed class WorkItemIdConverter()
    : ValueConverter<WorkItemId, Guid>(
        id => id.Value,
        value => WorkItemId.From(value));
