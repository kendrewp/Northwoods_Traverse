using Traverse.Domain.Primitives.Abstractions;

namespace Traverse.Domain.Primitives.ValueObjects;

/// <summary>
/// Postal address value object.
/// </summary>
/// <remarks>
/// All fields are required and validated in the compact constructor — an
/// <see cref="Address"/> instance can never be constructed in an invalid state,
/// which is the central guarantee value objects provide. Field-level
/// constraints are length-agnostic at this layer; persistence configuration in
/// the infrastructure project enforces column lengths.
/// </remarks>
public record Address(
    string Street,
    string City,
    string Province,
    string PostalCode,
    string Country) : ValueObject
{
    /// <summary>
    /// Compact constructor: validates that every field is non-null and
    /// non-whitespace. The constructor throws <see cref="ArgumentException"/>
    /// per field so the failure message identifies which property is invalid,
    /// not just "an address field was bad".
    /// </summary>
    public string Street { get; init; } = ValidateNotWhitespace(Street, nameof(Street));

    /// <inheritdoc cref="Street"/>
    public string City { get; init; } = ValidateNotWhitespace(City, nameof(City));

    /// <inheritdoc cref="Street"/>
    public string Province { get; init; } = ValidateNotWhitespace(Province, nameof(Province));

    /// <inheritdoc cref="Street"/>
    public string PostalCode { get; init; } = ValidateNotWhitespace(PostalCode, nameof(PostalCode));

    /// <inheritdoc cref="Street"/>
    public string Country { get; init; } = ValidateNotWhitespace(Country, nameof(Country));

    // Private validator avoids duplicating the null/whitespace check across five
    // properties (DRY). Returns the validated value so the property initialisers
    // can chain assignment + validation in a single expression.
    private static string ValidateNotWhitespace(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} cannot be null or whitespace", parameterName);
        }

        return value;
    }
}
