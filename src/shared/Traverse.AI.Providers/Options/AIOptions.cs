using System.ComponentModel.DataAnnotations;

namespace Traverse.AI.Providers.Options;

/// <summary>
/// Enumerates the AI providers that Traverse supports.
/// The active provider is chosen at startup via <c>AI:Provider</c> in configuration.
/// AWS Bedrock is the project default (PM decision, 2026-04-25).
/// </summary>
public enum AIProviderType
{
    /// <summary>
    /// Amazon Bedrock via the AWS SDK Converse API.
    /// Uses the AWS credential chain — no API key in configuration.
    /// Supports completions, streaming, and Amazon Titan embeddings.
    /// </summary>
    Bedrock,

    /// <summary>
    /// OpenAI direct API via the official OpenAI .NET SDK v2.
    /// Requires <c>AI:OpenAI:ApiKey</c> in secrets.
    /// Supports completions, streaming, and text-embedding models.
    /// </summary>
    OpenAI,

    /// <summary>
    /// Anthropic direct API via Anthropic.SDK.
    /// Requires <c>AI:Anthropic:ApiKey</c> in secrets.
    /// Supports completions and streaming only — no embeddings endpoint.
    /// </summary>
    Anthropic,
}

/// <summary>
/// Root options object bound from the <c>AI</c> configuration section.
/// Validated at startup via <c>ValidateOnStart()</c> so missing required values
/// surface immediately rather than at first use.
/// </summary>
public sealed class AIOptions
{
    /// <summary>Configuration section key.</summary>
    public const string Section = "AI";

    /// <summary>
    /// Which provider to activate. Defaults to <see cref="AIProviderType.Bedrock"/>
    /// because AWS Bedrock is the PM-selected default provider.
    /// </summary>
    public AIProviderType Provider { get; set; } = AIProviderType.Bedrock;

    /// <summary>AWS Bedrock provider settings.</summary>
    public BedrockOptions Bedrock { get; set; } = new();

    /// <summary>OpenAI provider settings.</summary>
    public OpenAIOptions OpenAI { get; set; } = new();

    /// <summary>Anthropic provider settings.</summary>
    public AnthropicOptions Anthropic { get; set; } = new();
}

/// <summary>
/// Settings specific to the AWS Bedrock provider.
/// No API key — authentication uses the AWS credential chain
/// (IAM role in production, environment variables or ~/.aws/credentials in development).
/// </summary>
public sealed class BedrockOptions
{
    /// <summary>
    /// AWS region where Bedrock endpoints are accessed.
    /// Must match the region where your model access is approved.
    /// </summary>
    [Required]
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// Bedrock model ID used for chat completions.
    /// Defaults to Claude 3.5 Sonnet v2, which supports the Converse API.
    /// Override with any Converse-API-compatible model identifier.
    /// </summary>
    [Required]
    public string ModelId { get; set; } = "anthropic.claude-3-5-sonnet-20241022-v2:0";

    /// <summary>
    /// Bedrock model ID used for embedding generation.
    /// Amazon Titan Embeddings V2 is the default; it produces 1024-dimensional vectors.
    /// </summary>
    [Required]
    public string EmbeddingModelId { get; set; } = "amazon.titan-embed-text-v2:0";
}

/// <summary>
/// Settings specific to the OpenAI provider.
/// The API key must be supplied via <c>dotnet user-secrets</c> in development or a
/// Kubernetes Secret / Azure Key Vault reference in production.  Never commit it to
/// <c>appsettings.json</c>.
/// </summary>
public sealed class OpenAIOptions
{
    /// <summary>
    /// OpenAI chat completion model.
    /// Defaults to GPT-4o.
    /// </summary>
    [Required]
    public string ModelId { get; set; } = "gpt-4o";

    /// <summary>
    /// OpenAI embedding model.
    /// text-embedding-3-small offers the best price/performance ratio for semantic search.
    /// </summary>
    [Required]
    public string EmbeddingModelId { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// OpenAI API key.  Populated from secrets — never from appsettings.json.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Settings specific to the Anthropic direct-API provider.
/// The API key must be supplied via secrets — never from appsettings.json.
/// Note: Anthropic does not provide an embeddings endpoint; use Bedrock or OpenAI
/// when embeddings are required alongside Anthropic models.
/// </summary>
public sealed class AnthropicOptions
{
    /// <summary>
    /// Anthropic model identifier.
    /// Defaults to Claude Sonnet 4.6, the current recommended production model.
    /// </summary>
    [Required]
    public string ModelId { get; set; } = "claude-sonnet-4-6";

    /// <summary>
    /// Anthropic API key.  Populated from secrets — never from appsettings.json.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
