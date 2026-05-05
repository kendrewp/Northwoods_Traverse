namespace Traverse.Domain.Primitives.Enums;

/// <summary>
/// Case taxonomy. Phase 0 ships only the <see cref="Unknown"/> guard value;
/// concrete service stories in Phase 1 add their own typed values to this enum
/// (or extend it with service-specific subtypes).
/// </summary>
/// <remarks>
/// The explicit <c>Unknown = 0</c> default ensures that any
/// default-initialised <see cref="CaseType"/> is a named value, not an
/// unnamed integer. This avoids the silent-zero problem where a forgotten
/// initialiser leaves an enum at numeric 0 with no semantic meaning.
/// </remarks>
public enum CaseType
{
    /// <summary>Default guard — indicates an unspecified or not-yet-classified case.</summary>
    Unknown = 0,
}
