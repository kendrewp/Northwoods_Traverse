using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter for <see cref="ProgramId"/>. See
/// <see cref="ClientIdConverter"/> for the rationale behind using
/// <see cref="ProgramId.From(Guid)"/> rather than the raw constructor.
/// </summary>
public sealed class ProgramIdConverter()
    : ValueConverter<ProgramId, Guid>(
        id => id.Value,
        value => ProgramId.From(value));
