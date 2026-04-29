using Traverse.Domain.Primitives.Abstractions;

namespace Traverse.Domain.Primitives.ValueObjects;

/// <summary>
/// Money value object — non-negative amount in a specified ISO currency code.
/// </summary>
/// <remarks>
/// <para>
/// The Traverse platform settles claims in CAD by default, so the
/// <see cref="CurrencyCode"/> parameter defaults to <c>"CAD"</c>. Callers who
/// transact in another currency must specify the code explicitly so the unit is
/// always present at the call site (DRY does not justify a hidden default).
/// </para>
/// <para>
/// Negative amounts are rejected because the domain has no concept of "negative
/// money" — debits and credits are modelled as separate transactions, not as
/// signed values.
/// </para>
/// </remarks>
public record Money(decimal Amount, string CurrencyCode = "CAD") : ValueObject
{
    /// <summary>Validated amount — guaranteed non-negative.</summary>
    public decimal Amount { get; init; } = ValidateAmount(Amount);

    /// <summary>Validated ISO currency code — guaranteed non-null/non-whitespace.</summary>
    public string CurrencyCode { get; init; } = ValidateCurrencyCode(CurrencyCode);

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        return amount;
    }

    private static string ValidateCurrencyCode(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            throw new ArgumentException("CurrencyCode cannot be null or whitespace", nameof(currencyCode));
        }

        return currencyCode;
    }
}
