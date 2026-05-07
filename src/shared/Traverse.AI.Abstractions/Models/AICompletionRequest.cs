namespace Traverse.AI.Abstractions.Models;

/// <summary>
/// All parameters required to request a chat completion from an AI provider.
/// System prompts are passed as <see cref="AIMessage.Role"/> = <see cref="AIMessageRole.System"/>
/// entries at the head of <see cref="Messages"/> — providers that separate system prompts
/// (e.g. Anthropic) extract them automatically inside their adapter.
/// </summary>
public sealed record AICompletionRequest
{
    /// <summary>
    /// The ordered conversation history, starting with any system prompt(s) followed by
    /// alternating User/Assistant turns.  Must contain at least one message.
    /// </summary>
    public required IReadOnlyList<AIMessage> Messages { get; init; }

    /// <summary>
    /// Maximum number of tokens the model may generate in its response.
    /// Defaults to 4096, which covers typical case-note and summary workloads.
    /// </summary>
    public int MaxTokens { get; init; } = 4_096;

    /// <summary>
    /// Sampling temperature (0.0 – 1.0).  Lower values produce more deterministic output,
    /// which is preferred for compliance-sensitive drafts.  Defaults to 0.3.
    /// </summary>
    public float Temperature { get; init; } = 0.3f;

    /// <summary>
    /// Caller-supplied model identifier override.  When null the provider uses the model
    /// configured via <c>AIOptions.{Provider}Options.ModelId</c>.
    /// </summary>
    public string? ModelIdOverride { get; init; }

    /// <summary>
    /// Opaque request identifier propagated to <see cref="AICompletionResponse.RequestId"/>
    /// and written to <c>AIInteractionLog</c> for audit traceability.
    /// </summary>
    public string? RequestId { get; init; }
}
