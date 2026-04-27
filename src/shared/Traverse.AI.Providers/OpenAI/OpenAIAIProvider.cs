using System.ClientModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Traverse.AI.Abstractions;
using Traverse.AI.Abstractions.Models;
using Traverse.AI.Providers.Options;

namespace Traverse.AI.Providers.OpenAI;

/// <summary>
/// AI provider adapter for the OpenAI direct API.
///
/// <para><b>SDK:</b> Uses the official OpenAI .NET SDK v2 (<c>OpenAI</c> NuGet package).
/// The SDK is model-agnostic — switching between gpt-4o, o1, and future models only
/// requires changing <c>AI:OpenAI:ModelId</c> in configuration.</para>
///
/// <para><b>Authentication:</b> API key is read from <see cref="OpenAIOptions.ApiKey"/>,
/// which must be supplied via <c>dotnet user-secrets</c> in development and a Kubernetes
/// Secret or Key Vault reference in production.  It must never appear in appsettings.json.</para>
/// </summary>
internal sealed class OpenAIAIProvider : IAIProvider
{
    private readonly ChatClient _chatClient;
    private readonly EmbeddingClient _embeddingClient;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIAIProvider> _logger;

    /// <inheritdoc />
    public string ProviderName => "OpenAI";

    /// <inheritdoc />
    public bool SupportsEmbeddings => true;

    /// <summary>
    /// Initialises the provider and creates typed SDK clients for the configured models.
    /// Separate <see cref="ChatClient"/> and <see cref="EmbeddingClient"/> instances are
    /// created because the OpenAI SDK binds each client to one model at construction time.
    /// </summary>
    public OpenAIAIProvider(IOptions<AIOptions> options, ILogger<OpenAIAIProvider> logger)
    {
        _options = options.Value.OpenAI;
        _logger = logger;

        var openAIClient = new OpenAIClient(_options.ApiKey);
        _chatClient = openAIClient.GetChatClient(_options.ModelId);
        _embeddingClient = openAIClient.GetEmbeddingClient(_options.EmbeddingModelId);
    }

    /// <inheritdoc />
    public async Task<AICompletionResponse> CompleteAsync(
        AICompletionRequest request,
        CancellationToken ct = default)
    {
        // ModelIdOverride cannot be applied once ChatClient is bound to a model at construction.
        // If override support is required in future, construct a new ChatClient per call.
        var modelId = request.ModelIdOverride ?? _options.ModelId;
        var chatMessages = BuildChatMessages(request.Messages);
        var chatOptions = BuildChatOptions(request);

        try
        {
            var response = await _chatClient.CompleteChatAsync(chatMessages, chatOptions, ct);
            var completion = response.Value;

            var content = string.Concat(completion.Content.Select(c => c.Text));
            var stopReason = MapStopReason(completion.FinishReason);
            var usage = new AIUsage(
                completion.Usage.InputTokenCount,
                completion.Usage.OutputTokenCount);

            _logger.LogInformation(
                "OpenAI completion: model={ModelId} in={InputTokens} out={OutputTokens} stop={StopReason}",
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
            _logger.LogError(ex, "OpenAI CompleteChatAsync failed for model {ModelId}", modelId);
            throw new AIProviderException(ProviderName, ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AICompletionRequest request,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var chatMessages = BuildChatMessages(request.Messages);
        var chatOptions = BuildChatOptions(request);

        AsyncCollectionResult<StreamingChatCompletionUpdate> stream;
        try
        {
            stream = _chatClient.CompleteChatStreamingAsync(chatMessages, chatOptions, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "OpenAI CompleteChatStreamingAsync failed");
            throw new AIProviderException(ProviderName, ex.Message, ex);
        }

        await foreach (var update in stream.WithCancellation(ct))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                    yield return part.Text;
            }
        }
    }

    /// <inheritdoc />
    public async Task<AIEmbeddingResponse> EmbedAsync(
        AIEmbeddingRequest request,
        CancellationToken ct = default)
    {
        var modelId = request.ModelIdOverride ?? _options.EmbeddingModelId;

        try
        {
            // The OpenAI SDK accepts multiple inputs in a single call (up to 2048).
            var response = await _embeddingClient.GenerateEmbeddingsAsync(
                request.Inputs.ToList(), cancellationToken: ct);

            var embeddings = response.Value
                .Select((e, i) => new EmbeddingVector(i, e.ToFloats()))
                .ToList();

            var totalTokens = response.Value.Sum(e => e.Index >= 0 ? 1 : 0); // token count not exposed per-item in v2

            _logger.LogInformation(
                "OpenAI embeddings: model={ModelId} count={Count}", modelId, embeddings.Count);

            return new AIEmbeddingResponse
            {
                Embeddings = embeddings,
                ModelId = modelId,
                InputTokens = totalTokens,
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "OpenAI GenerateEmbeddingsAsync failed for model {ModelId}", modelId);
            throw new AIProviderException(ProviderName, ex.Message, ex);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Converts the provider-agnostic <see cref="AIMessage"/> list to OpenAI SDK
    /// <see cref="ChatMessage"/> objects.  The SDK uses a type-per-role hierarchy.
    /// </summary>
    private static List<ChatMessage> BuildChatMessages(IReadOnlyList<AIMessage> messages) =>
        messages.Select<AIMessage, ChatMessage>(m => m.Role switch
        {
            AIMessageRole.System    => new SystemChatMessage(m.Content),
            AIMessageRole.User      => new UserChatMessage(m.Content),
            AIMessageRole.Assistant => new AssistantChatMessage(m.Content),
            _ => throw new ArgumentOutOfRangeException(nameof(m.Role), m.Role, "Unexpected role."),
        }).ToList();

    /// <summary>Builds SDK completion options from the provider-agnostic request.</summary>
    private static ChatCompletionOptions BuildChatOptions(AICompletionRequest request) =>
        new()
        {
            MaxOutputTokenCount = request.MaxTokens,
            Temperature = request.Temperature,
        };

    /// <summary>Maps OpenAI finish reason values to the provider-agnostic enum.</summary>
    private static AIStopReason MapStopReason(ChatFinishReason? reason) => reason switch
    {
        ChatFinishReason.Stop         => AIStopReason.EndTurn,
        ChatFinishReason.Length       => AIStopReason.MaxTokens,
        null                          => AIStopReason.Unknown,
        _                             => AIStopReason.Unknown,
    };
}
