using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Traverse.Domain.Primitives.Identity;

namespace Traverse.Infrastructure.Persistence.Converters;

/// <summary>
/// EF Core value converter mapping <see cref="ClientId"/> to/from <see cref="Guid"/>.
/// </summary>
/// <remarks>
/// The reverse direction calls <see cref="ClientId.From(Guid)"/> rather than the
/// raw record-struct constructor so that an empty Guid stored in the database
/// fails loudly instead of silently propagating a default-valued identity. This
/// is defensive programming: if the database ever holds bad data, surfacing it
/// at materialisation time is far cheaper to diagnose than silent downstream
/// breakage.
/// </remarks>
public sealed class ClientIdConverter()
    : ValueConverter<ClientId, Guid>(
        id => id.Value,
        value => ClientId.From(value));
