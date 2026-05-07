namespace Traverse.Domain.Primitives.Identity;

/// <summary>
/// Strongly-typed identifier for a User aggregate. See <see cref="ClientId"/>
/// for the rationale behind the <c>readonly record struct</c> + factory pattern.
/// </summary>
public readonly record struct UserId(Guid Value)
{
    /// <summary>Generates a fresh <see cref="UserId"/>.</summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/>; rejects <see cref="Guid.Empty"/>.</summary>
    public static UserId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty", nameof(value));
        }

        return new UserId(value);
    }

    /// <inheritdoc cref="ClientId.ToString"/>
    public override string ToString() => Value.ToString();
}
