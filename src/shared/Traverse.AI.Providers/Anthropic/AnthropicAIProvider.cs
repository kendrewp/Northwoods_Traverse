using System.Runtime.CompilerServices;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Traverse.AI.Abstractions;
using Traverse.AI.Abstractions.Models;
using Traverse.AI.Providers.Options;
using AnthropicMessage = Anthropic.SDK.Messaging.Message;

namespace Traverse.AI.Providers.Anthropic;

/// <summary>
/// AI provider adapter for the Anthropic direct API.
///
/// <para><b>SDK:</b> Uses Anthropic.SDK (unofficial but well-maintained .NET client).
/// Supports the Messages API for completions and streaming.</para>
///
/// <para><b>No embeddings:</b> Anthropic does not publish an embeddings endpoint.
/// <see cref="SupportsEmbeddings"/> is <c>false</c>.  Callers that require both
/// Anthropic models and embeddings must use Bedrock (which hosts Claude models and
/// provides Titan embeddings) or switch to a mixed-provider approach.</para>
///
/// <para><b>Authentication:</b> API key is read from <see cref="AnthropicOptions.ApiKey"/>,
/// supplied via secrets — never from appsettings.json.</para>
/// </summary>
internal sealed class AnthropicAIProvider : IAIProvider
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;
    private readonly ILogger<AnthropicAIProvider> _logger;

    /// <inheritdoc />
    public string ProviderName => "Anthropic";

    /// <inheritdoc />
    /// <remarks>
    /// Anthropic does not offer an embeddings API.  If your deployment requires
    /// Anthropic models but also needs semantic search or RAG, use AWS Bedrock instead —
    /// it hosts the same Claude models and also provides Amazon Titan embeddings.
    /// </remarks>
    public bool SupportsEmbeddings => false;

    /// <summary>
    /// Initialises the Anthropic client with the configured API key.
    /// </summary>
    public AnthropicAIProvider(IOptions<AIOptions> options, ILogger<AnthropicAIProvider> logger)
    {
        _options = options.Value.Anthropic;
        _logger = logger;
        _client = new AnthropicClient(_options.ApiKey);
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(
        AICompletionRequest request,
        CancellationToken ct = default)
    {
        var modelId = request.ModelIdOverride ?? _options.ModelId;
        var (systemPrompt, messages) = SplitMessages(request.Messages);

        // Anthropic.SDK 3.x uses SystemMessage as a string property, not a List<SystemMessage>.
        // The System property was renamed to SystemMessage in the 3.x API.
        var parameters = new MessageParameters
        {
            Model = modelId,
            MaxTokens = request.MaxTokens,
            Temperature = (decimal)request.Temperature,
            SystemMessage = systemPrompt,
            Messages = messages,
        };

        try
        {
            // Anthropic.SDK 3.3.0: GetClaudeMessageAsync(parameters, tools, ct) — pass null for no tools.
            var response = await _client.Messages.GetClaudeMessageAsync(parameters, null, ct);

            var content = string.Concat(response.Content
                .OfType<TextContent>()
                .Select(c => c.Text));

            var stopReason = MapStopReason(response.StopReason);
            var usage = new AIUsage(response.Usage.InputTokens, response.Usage.OutputTokens);

            _logger.LogInformation(
                "Anthropic completion: model={ModelId} in={InputTokens} out={OutputTokens} stop={StopReason}",
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
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Anthropic GetClaudeMessageAsync failed for model {ModelId}", modelId);
            throw new AIProviderException(ProviderName, ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AICompletionRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var modelId = request.ModelIdOverride ?? _options.ModelId;
        var (systemPrompt, messages) = SplitMessages(request.Messages);

        // Anthropic.SDK 3.x uses SystemMessage as a string property, not a List<SystemMessage>.
        // The System property was renamed to SystemMessage in the 3.x API.
        var parameters = new MessageParameters
        {
            Model = modelId,
            MaxTokens = request.MaxTokens,
            Temperature = (decimal)request.Temperature,
            SystemMessage = systemPrompt,
            Messages = messages,
            Stream = true,
        };

        // In Anthropic.SDK 3.3.x, StreamClaudeMessageAsync returns IAsyncEnumerable<MessageResponse>.
        // Each MessageResponse has a Delta property with the incremental text fragment.
        // Signature: StreamClaudeMessageAsync(parameters, tools, ct) — pass null for no tools.
        IAsyncEnumerable<MessageResponse> stream;
        try
        {
            stream = _client.Messages.StreamClaudeMessageAsync(parameters, null, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Anthropic StreamClaudeMessageAsync failed for model {ModelId}", modelId);
            throw new AIProviderException(ProviderName, ex.Message, ex);
        }

        await foreach (var evt in stream.WithCancellation(ct))
        {
            // MessageResponse.Delta.Text contains incremental text fragments.
            // Other event types (MessageStart, MessageStop) have null or empty Delta.Text.
            if (evt.Delta?.Text is { Length: > 0 } text)
            {
                yield return text;
            }
        }
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">
    /// Always thrown — Anthropic does not provide an embeddings endpoint.
    /// Use AWS Bedrock or OpenAI for embedding generation.
    /// </exception>
    public Task<AIEmbeddingResponse> EmbedAsync(AIEmbeddingRequest request, CancellationToken ct = default) =>
        throw new NotSupportedException(
            "The Anthropic provider does not support embedding generation. " +
            "Switch to AIProviderType.Bedrock or AIProviderType.OpenAI for embedding workloads.");

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Extracts any leading system message and converts the remaining history to
    /// Anthropic SDK <see cref="AnthropicMessage"/> objects.  Only the first system
    /// message is promoted — multiple system messages are concatenated.
    /// </summary>
    private static (string? SystemPrompt, List<AnthropicMessage> Messages) SplitMessages(
        IReadOnlyList<AIMessage> messages)
    {
        var systemParts = messages
            .Where(m => m.Role == AIMessageRole.System)
            .Select(m => m.Content)
            .ToList();

        var systemPrompt = systemParts.Count > 0
            ? string.Join("\n\n", systemParts)
            : (string?)null;

        var conversationMessages = messages
            .Where(m => m.Role != AIMessageRole.System)
            .Select(m => new AnthropicMessage
            {
                Role = m.Role == AIMessageRole.User ? RoleType.User : RoleType.Assistant,
                Content = [new TextContent { Text = m.Content }],
            })
            .ToList();

        return (systemPrompt, conversationMessages);
    }

    /// <summary>Maps Anthropic stop reason strings to the provider-agnostic enum.</summary>
    private static AIStopReason MapStopReason(string? reason) => reason switch
    {
        "end_turn"   => AIStopReason.EndTurn,
        "max_tokens" => AIStopReason.MaxTokens,
        _            => AIStopReason.Unknown,
    };
}
