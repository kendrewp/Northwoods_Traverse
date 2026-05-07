namespace Traverse.Domain.Primitives.Identity;

/// <summary>
/// Strongly-typed identifier for a Case aggregate. See <see cref="ClientId"/>
/// for the rationale behind the <c>readonly record struct</c> + factory pattern.
/// </summary>
public readonly record struct CaseId(Guid Value)
{
    /// <summary>Generates a fresh <see cref="CaseId"/>.</summary>
    public static CaseId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/>; rejects <see cref="Guid.Empty"/>.</summary>
    public static CaseId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CaseId cannot be empty", nameof(value));
        }

        return new CaseId(value);
    }

    /// <inheritdoc cref="ClientId.ToString"/>
    public override string ToString() => Value.ToString();
}
