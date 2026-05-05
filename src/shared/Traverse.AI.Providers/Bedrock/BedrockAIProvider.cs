using System.Runtime.CompilerServices;
using System.Text.Json;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traverse.AI.Abstractions;
using Traverse.AI.Abstractions.Models;
using Traverse.AI.Providers.Options;

namespace Traverse.AI.Providers.Bedrock;

/// <summary>
/// AI provider adapter for AWS Bedrock.
///
/// <para><b>Completions:</b> Uses the Bedrock Converse API (<c>ConverseAsync</c> /
/// <c>ConverseStreamAsync</c>).  The Converse API is model-agnostic — it works with
/// Claude, Titan, Mistral, and other Bedrock models without SDK changes, which is why
/// it was chosen over the lower-level InvokeModel endpoint for completions.</para>
///
/// <para><b>Embeddings:</b> Uses <c>InvokeModelAsync</c> with the Amazon Titan Embeddings
/// model because the Converse API does not support embedding generation.  The request
/// payload is the Titan V2 JSON schema <c>{ "inputText": "..." }</c>.</para>
///
/// <para><b>Authentication:</b> Uses the AWS SDK default credential chain
/// (instance role → environment variables → ~/.aws/credentials).  No API key is stored
/// in application configuration — this is intentional for security compliance.</para>
/// </summary>
internal sealed class BedrockAIProvider : IAIProvider
{
    private readonly IAmazonBedrockRuntime _client;
    private readonly BedrockOptions _options;
    private readonly ILogger<BedrockAIProvider> _logger;

    /// <inheritdoc />
    public string ProviderName => "Bedrock";

    /// <inheritdoc />
    public bool SupportsEmbeddings => true;

    /// <summary>
    /// Initialises the provider.  The <see cref="IAmazonBedrockRuntime"/> client is
    /// injected so it can be replaced with a mock in tests without hitting AWS.
    /// </summary>
    public BedrockAIProvider(
        IAmazonBedrockRuntime client,
        IOptions<AIOptions> options,
        ILogger<BedrockAIProvider> logger)
    {
        _client = client;
        _options = options.Value.Bedrock;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(
        AICompletionRequest request,
        CancellationToken ct = default)
    {
        var modelId = request.ModelIdOverride ?? _options.ModelId;

        try
        {
            var converseRequest = BuildConverseRequest(request, modelId);
            var response = await _client.ConverseAsync(converseRequest, ct);

            var content = ExtractTextFromContent(response.Output.Message.Content);
            var stopReason = MapStopReason(response.StopReason);
            var usage = new AIUsage(
                response.Usage.InputTokens,
                response.Usage.OutputTokens);

            _logger.LogInformation(
                "Bedrock completion: model={ModelId} in={InputTokens} out={OutputTokens} stop={StopReason}",
                modelId, usage.InputTokens, usage.OutputTokens, stopReason);

            return new AICompletionResponse
            {
                Content = content,
                StopReason = stopReason,
                Usage = usage,
                ModelId = modelId,
                RequestId = request.RequestId,
            };
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            _logger.LogError(ex, "Bedrock ConverseAsync failed for model {ModelId}", modelId);
            throw new AIProviderException(ProviderName, ex.StatusCode.GetHashCode(), ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AICompletionRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var modelId = request.ModelIdOverride ?? _options.ModelId;
        ConverseStreamResponse streamResponse;

        try
        {
            var converseRequest = BuildConverseStreamRequest(request, modelId);
            streamResponse = await _client.ConverseStreamAsync(converseRequest, ct);
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            _logger.LogError(ex, "Bedrock ConverseStreamAsync failed for model {ModelId}", modelId);
            throw new AIProviderException(ProviderName, ex.StatusCode.GetHashCode(), ex.Message, ex);
        }

        // ConverseStreamOutput implements IEnumerable<IEventStreamEvent> (synchronous).
        // Enumerate on a thread pool thread and yield back to the async context.
        // ContentBlockDelta.Text is a string property (not a union nested type) in SDK 3.7.514+.
        foreach (var evt in streamResponse.Stream)
        {
            ct.ThrowIfCancellationRequested();
            // ContentBlockDeltaEvent carries incremental text fragments.
            // Other event types (MessageStart, MessageStop, Metadata) are intentionally ignored
            // — the caller only needs the text stream.
            if (evt is ContentBlockDeltaEvent deltaEvent &&
                deltaEvent.Delta?.Text is { Length: > 0 } textDelta)
            {
                yield return textDelta;
            }
        }
    }

    /// <inheritdoc />
    public async Task<AIEmbeddingResponse> EmbedAsync(
        AIEmbeddingRequest request,
        CancellationToken ct = default)
    {
        var modelId = request.ModelIdOverride ?? _options.EmbeddingModelId;
        var embeddings = new List<EmbeddingVector>(request.Inputs.Count);
        var totalTokens = 0;

        // Amazon Titan Embeddings V2 accepts exactly one inputText per call.
        // We issue one request per input and collect results in order.
        for (var i = 0; i < request.Inputs.Count; i++)
        {
            var payload = new { inputText = request.Inputs[i] };
            var bodyBytes = JsonSerializer.SerializeToUtf8Bytes(payload);

            InvokeModelResponse response;
            try
            {
                response = await _client.InvokeModelAsync(new InvokeModelRequest
                {
                    ModelId = modelId,
                    ContentType = "application/json",
                    Accept = "application/json",
                    Body = new MemoryStream(bodyBytes),
                }, ct);
            }
            catch (AmazonBedrockRuntimeException ex)
            {
                _logger.LogError(ex, "Bedrock InvokeModelAsync failed for embedding model {ModelId}", modelId);
                throw new AIProviderException(ProviderName, ex.StatusCode.GetHashCode(), ex.Message, ex);
            }

            var titanResponse = await JsonSerializer.DeserializeAsync<TitanEmbeddingResponse>(
                response.Body, cancellationToken: ct)
                ?? throw new AIProviderException(ProviderName, "Bedrock returned a null embedding response.");

            embeddings.Add(new EmbeddingVector(i, titanResponse.Embedding.AsMemory()));
            totalTokens += titanResponse.InputTextTokenCount;
        }

        return new AIEmbeddingResponse
        {
            Embeddings = embeddings,
            ModelId = modelId,
            InputTokens = totalTokens,
        };
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Builds a <see cref="ConverseRequest"/> by separating system messages from
    /// the conversation history.  The Converse API requires system content in a
    /// dedicated top-level field rather than as a message in the list.
    /// </summary>
    private static ConverseRequest BuildConverseRequest(AICompletionRequest request, string modelId)
    {
        var (systemContent, conversationMessages) = SplitMessages(request.Messages);

        return new ConverseRequest
        {
            ModelId = modelId,
            System = systemContent,
            Messages = conversationMessages,
            InferenceConfig = new InferenceConfiguration
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature,
            },
        };
    }

    /// <summary>Builds a <see cref="ConverseStreamRequest"/> using the same message split.</summary>
    private static ConverseStreamRequest BuildConverseStreamRequest(AICompletionRequest request, string modelId)
    {
        var (systemContent, conversationMessages) = SplitMessages(request.Messages);

        return new ConverseStreamRequest
        {
            ModelId = modelId,
            System = systemContent,
            Messages = conversationMessages,
            InferenceConfig = new InferenceConfiguration
            {
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature,
            },
        };
    }

    /// <summary>
    /// Partitions the message list into Bedrock system content blocks and user/assistant
    /// conversation turns.  System messages are extracted and promoted to the Converse
    /// API's top-level <c>System</c> field.
    /// </summary>
    private static (List<SystemContentBlock> SystemContent, List<Message> Messages) SplitMessages(
        IReadOnlyList<AIMessage> messages)
    {
        var systemContent = messages
            .Where(m => m.Role == AIMessageRole.System)
            .Select(m => new SystemContentBlock { Text = m.Content })
            .ToList();

        var conversationMessages = messages
            .Where(m => m.Role != AIMessageRole.System)
            .Select(m => new Message
            {
                Role = m.Role == AIMessageRole.User ? ConversationRole.User : ConversationRole.Assistant,
                Content = [new ContentBlock { Text = m.Content }],
            })
            .ToList();

        return (systemContent, conversationMessages);
    }

    /// <summary>Concatenates all text blocks from a Bedrock response message into a single string.</summary>
    private static string ExtractTextFromContent(List<ContentBlock> content) =>
        string.Concat(content
            .Where(b => b.Text is not null)
            .Select(b => b.Text));

    /// <summary>Translates Bedrock stop reason values to the provider-agnostic enum.</summary>
    private static AIStopReason MapStopReason(StopReason? reason) => reason?.Value switch
    {
        "end_turn"   => AIStopReason.EndTurn,
        "max_tokens" => AIStopReason.MaxTokens,
        _            => AIStopReason.Unknown,
    };

    /// <summary>
    /// Deserialisation target for the Amazon Titan Embeddings V2 response payload.
    /// Schema: <c>{ "embedding": [...], "inputTextTokenCount": N }</c>
    /// </summary>
    private sealed record TitanEmbeddingResponse(
        float[] Embedding,
        int InputTextTokenCount);
}
