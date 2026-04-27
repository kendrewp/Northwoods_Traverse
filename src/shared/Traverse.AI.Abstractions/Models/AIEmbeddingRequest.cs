namespace Traverse.AI.Abstractions.Models;

/// <summary>
/// Parameters for a vector embedding request.
/// Embeddings are used by the Semantic Search (MOD-05) and Policy Q&amp;A (FR-AI-008)
/// features to retrieve contextually relevant case records and policy documents.
/// </summary>
public sealed record AIEmbeddingRequest
{
    /// <summary>
    /// One or more strings to embed.  Batching multiple inputs in a single call is more
    /// efficient than issuing one request per text, but batch size limits vary by provider:
    /// Amazon Titan accepts one input per call; OpenAI accepts up to 2048.
    /// Callers should chunk accordingly before constructing this request.
    /// </summary>
    public required IReadOnlyList<string> Inputs { get; init; }

    /// <summary>
    /// Caller-supplied model identifier override.  When null the provider uses the model
    /// configured via <c>AIOptions.{Provider}Options.EmbeddingModelId</c>.
    /// </summary>
    public string? ModelIdOverride { get; init; }
}
