namespace Traverse.Domain.Primitives.Identity;

/// <summary>
/// Strongly-typed identifier for a Client aggregate.
/// </summary>
/// <remarks>
/// Implemented as a <c>readonly record struct</c> so it is allocation-free in
/// the common pass-by-value path while still gaining record value-equality
/// semantics. Wrapping <see cref="Guid"/> in a domain-named type prevents
/// accidental cross-aggregate ID confusion at compile time (e.g., passing a
/// <see cref="CaseId"/> where a <see cref="ClientId"/> is required).
/// </remarks>
public readonly record struct ClientId(Guid Value)
{
    /// <summary>Generates a fresh <see cref="ClientId"/>.</summary>
    public static ClientId New() => new(Guid.NewGuid());

    /// <summary>
    /// Wraps an existing <see cref="Guid"/> as a <see cref="ClientId"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/> — used
    /// defensively against database corruption or untyped wire payloads.
    /// </exception>
    public static ClientId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ClientId cannot be empty", nameof(value));
        }

        return new ClientId(value);
    }

    /// <summary>String form is the underlying GUID — useful for log output.</summary>
    public override string ToString() => Value.ToString();
}
