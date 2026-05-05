using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter for <see cref="UserId"/>. See
/// <see cref="ClientIdConverter"/> for the rationale behind using
/// <see cref="UserId.From(Guid)"/> rather than the raw constructor.
/// </summary>
public sealed class UserIdConverter()
    : ValueConverter<UserId, Guid>(
        id => id.Value,
        value => UserId.From(value));
