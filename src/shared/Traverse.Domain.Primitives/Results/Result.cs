namespace Traverse.Domain.Primitives.Results;

/// <summary>
/// Discriminated union representing the outcome of an operation that can either
/// succeed with a value of type <typeparamref name="T"/> or fail with a
/// machine-readable error code and human-readable message.
/// </summary>
/// <remarks>
/// <para>
/// Used for <em>expected</em> failure cases — operations whose failure is part
/// of normal control flow (for example "user not found" when the caller will
/// decide whether to 404 or fall back to a default). Unexpected failures
/// (bugs, infrastructure outages) should still throw — exceptions remain the
/// right tool for exceptional cases.
/// </para>
/// <para>
/// Modelled as an <c>abstract</c> record with sealed nested records so callers
/// can pattern-match exhaustively. The <see cref="Match{TOut}"/> helper makes
/// pattern coverage a compile-time concern: anyone who calls it must supply
/// both branches.
/// </para>
/// </remarks>
/// <typeparam name="T">Type of the success payload.</typeparam>
public abstract record Result<T>
{
    // Private constructor prevents external code from defining a third subtype,
    // which would break the exhaustiveness assumption that Match() relies on.
    private Result()
    {
    }

    /// <summary>Successful outcome carrying the produced value.</summary>
    public sealed record Success(T Data) : Result<T>;

    /// <summary>Failed outcome with a stable error code and a description.</summary>
    public sealed record Failure(string ErrorCode, string Message) : Result<T>;

    /// <summary>
    /// Pattern-match helper. Forces the caller to handle both outcomes in a
    /// single expression, which is the main practical benefit of the discriminated
    /// union pattern over plain ad-hoc <c>if (result is Success ...)</c> chains.
    /// </summary>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string, string, TOut> onFailure) =>
        this switch
        {
            Success s => onSuccess(s.Data),
            Failure f => onFailure(f.ErrorCode, f.Message),
            // The default case can never execute because the private
            // constructor forbids new subtypes. It exists to satisfy the
            // C# exhaustiveness analyser and to fail loudly if someone bypasses
            // the constraint via reflection.
            _ => throw new InvalidOperationException("Unexpected Result subtype"),
        };
}
