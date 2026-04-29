namespace Traverse.Domain.Primitives.Identity;

/// <summary>
/// Strongly-typed identifier for a Program aggregate. See <see cref="ClientId"/>
/// for the rationale behind the <c>readonly record struct</c> + factory pattern.
/// </summary>
public readonly record struct ProgramId(Guid Value)
{
    /// <summary>Generates a fresh <see cref="ProgramId"/>.</summary>
    public static ProgramId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/>; rejects <see cref="Guid.Empty"/>.</summary>
    public static ProgramId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ProgramId cannot be empty", nameof(value));
        }

        return new ProgramId(value);
    }

    /// <inheritdoc cref="ClientId.ToString"/>
    public override string ToString() => Value.ToString();
}
