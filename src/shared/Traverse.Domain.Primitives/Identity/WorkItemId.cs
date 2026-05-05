namespace Traverse.Domain.Primitives.Identity;

/// <summary>
/// Strongly-typed identifier for a WorkItem aggregate. See <see cref="ClientId"/>
/// for the rationale behind the <c>readonly record struct</c> + factory pattern.
/// </summary>
public readonly record struct WorkItemId(Guid Value)
{
    /// <summary>Generates a fresh <see cref="WorkItemId"/>.</summary>
    public static WorkItemId New() => new(Guid.NewGuid());

    /// <summary>Wraps an existing <see cref="Guid"/>; rejects <see cref="Guid.Empty"/>.</summary>
    public static WorkItemId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("WorkItemId cannot be empty", nameof(value));
        }

        return new WorkItemId(value);
    }

    /// <inheritdoc cref="ClientId.ToString"/>
    public override string ToString() => Value.ToString();
}
