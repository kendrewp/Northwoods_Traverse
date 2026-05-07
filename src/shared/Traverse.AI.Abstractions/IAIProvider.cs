using Traverse.AI.Abstractions.Models;

namespace Traverse.AI.Abstractions;

/// <summary>
/// Contract that every AI provider adapter must fulfil.
/// Implementations must be registered as <c>IAIProvider</c> singletons so the
/// active provider can be switched via configuration without touching call sites.
///
/// <para><b>Provider support matrix:</b></para>
/// <list type="table">
///   <listheader><term>Provider</term><description>Completions / Streaming / Embeddings</description></listheader>
///   <item><term>AWS Bedrock</term><description>Yes / Yes / Yes (Amazon Titan)</description></item>
///   <item><term>OpenAI</term><description>Yes / Yes / Yes</description></item>
///   <item><term>Anthropic</term><description>Yes / Yes / No</description></item>
/// </list>
///
/// <para>
/// Call sites that require embeddings MUST check <see cref="SupportsEmbeddings"/> before
/// calling <see cref="EmbedAsync"/> and throw or fall back appropriately.
/// </para>
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Human-readable provider identifier (e.g. "Bedrock", "OpenAI", "Anthropic").
    /// Stored in <c>AIInteractionLog.Provider</c> for audit purposes.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Whether this provider supports vector embedding generation.
    /// Anthropic direct does not expose an embeddings endpoint.
    /// </summary>
    bool SupportsEmbeddings { get; }

    /// <summary>
    /// Sends a chat completion request and returns the full response once the model
    /// has finished generating.  Use <see cref="StreamCompleteAsync"/> when the UI
    /// needs to display incremental output.
    /// </summary>
    /// <param name="request">Conversation history, parameters, and optional overrides.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>The model's complete response with token usage.</returns>
    /// <exception cref="AIProviderException">
    /// Thrown for provider-level errors (rate limit, quota exceeded, unavailable model).
    /// Callers should handle this to activate degraded mode per AC-11.8 in the AI Copilot PRD.
    /// </exception>
    Task<AICompletionResponse> CompleteAsync(AICompletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Sends a chat completion request and streams the model's response token-by-token.
    /// Each yielded string is a raw text fragment — callers are responsible for assembling
    /// the full response if needed.
    /// </summary>
    /// <param name="request">Conversation history, parameters, and optional overrides.</param>
    /// <param name="ct">Propagated cancellation token — disposing or cancelling stops the stream.</param>
    /// <returns>An async sequence of text fragments as they arrive from the provider.</returns>
    /// <exception cref="AIProviderException">Thrown for provider-level errors.</exception>
    IAsyncEnumerable<string> StreamCompleteAsync(AICompletionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates dense vector embeddings for one or more text inputs.
    /// Only call this when <see cref="SupportsEmbeddings"/> is <c>true</c>.
    /// </summary>
    /// <param name="request">Texts to embed and optional model override.</param>
    /// <param name="ct">Propagated cancellation token.</param>
    /// <returns>Embedding vectors in input order.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <see cref="SupportsEmbeddings"/> is <c>false</c>.
    /// </exception>
    /// <exception cref="AIProviderException">Thrown for provider-level errors.</exception>
    Task<AIEmbeddingResponse> EmbedAsync(AIEmbeddingRequest request, CancellationToken ct = default);
}
