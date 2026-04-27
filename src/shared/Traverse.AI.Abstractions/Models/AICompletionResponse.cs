namespace Traverse.AI.Abstractions.Models;

/// <summary>
/// The reason an AI model stopped generating tokens.
/// </summary>
public enum AIStopReason
{
    /// <summary>The model finished the response naturally.</summary>
    EndTurn,

    /// <summary>The response was cut short by the <c>MaxTokens</c> limit.</summary>
    MaxTokens,

    /// <summary>The provider returned an unknown or unrecognised stop reason.</summary>
    Unknown,
}

/// <summary>
/// Token consumption for a single completion request.  Used to feed the
/// <c>AIInteractionLog</c> cost-tracking columns required by the PRD.
/// </summary>
/// <param name="InputTokens">Tokens consumed by the prompt and conversation history.</param>
/// <param name="OutputTokens">Tokens generated in the model's response.</param>
public sealed record AIUsage(int InputTokens, int OutputTokens)
{
    /// <summary>Total tokens consumed (input + output).</summary>
    public int TotalTokens => InputTokens + OutputTokens;
}

/// <summary>
/// The result returned by <see cref="IAIProvider.CompleteAsync"/>.
/// </summary>
public sealed record AICompletionResponse
{
    /// <summary>The model's generated text.</summary>
    public required string Content { get; init; }

    /// <summary>Why the model stopped generating.</summary>
    public required AIStopReason StopReason { get; init; }

    /// <summary>Token usage for cost attribution and logging.</summary>
    public required AIUsage Usage { get; init; }

    /// <summary>The model identifier actually used (may differ from the requested override).</summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Echoed from <see cref="AICompletionRequest.RequestId"/> so callers can correlate
    /// log entries without additional state.
    /// </summary>
    public string? RequestId { get; init; }
}
