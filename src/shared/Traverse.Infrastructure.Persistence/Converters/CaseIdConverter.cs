using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter for <see cref="CaseId"/>. See
/// <see cref="ClientIdConverter"/> for the rationale behind using
/// <see cref="CaseId.From(Guid)"/> rather than the raw constructor.
/// </summary>
public sealed class CaseIdConverter()
    : ValueConverter<CaseId, Guid>(
        id => id.Value,
        value => CaseId.From(value));
