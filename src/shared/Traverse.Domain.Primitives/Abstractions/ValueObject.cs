namespace Traverse.Domain.Primitives.Abstractions;

/// <summary>
/// Marker base for value objects.
/// </summary>
/// <remarks>
/// <para>
/// Modelled as an empty abstract <c>record</c> because the C# <c>record</c>
/// keyword already provides the structural-equality contract that defines a
/// value object: two instances with identical attribute values are
/// indistinguishable. There are no shared methods to add at this layer —
/// the type exists for discoverability (it documents the role) and as an
/// extension seam if cross-cutting concerns ever need to operate on "all
/// value objects".
/// </para>
/// </remarks>
public abstract record ValueObject;
