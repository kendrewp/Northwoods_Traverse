namespace Traverse.AI.Abstractions.Models;

/// <summary>
/// A single embedding vector paired with its source text index.
/// </summary>
/// <param name="Index">Zero-based position corresponding to <see cref="AIEmbeddingRequest.Inputs"/>.</param>
/// <param name="Vector">The dense float vector representation of the input text.</param>
public sealed record EmbeddingVector(int Index, ReadOnlyMemory<float> Vector);

/// <summary>
/// The result returned by <see cref="IAIProvider.EmbedAsync"/>.
/// </summary>
public sealed record AIEmbeddingResponse
{
    /// <summary>
    /// Embedding vectors in the same order as <see cref="AIEmbeddingRequest.Inputs"/>.
    /// </summary>
    public required IReadOnlyList<EmbeddingVector> Embeddings { get; init; }

    /// <summary>The embedding model identifier that produced these vectors.</summary>
    public required string ModelId { get; init; }

    /// <summary>Total input tokens consumed, for cost attribution.</summary>
    public int InputTokens { get; init; }
}
