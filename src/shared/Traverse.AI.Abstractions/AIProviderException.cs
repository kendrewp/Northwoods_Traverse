namespace Traverse.AI.Abstractions;

/// <summary>
/// Raised when an AI provider returns an error or is unavailable.
/// Catch this exception at feature boundaries to activate degraded-mode fallbacks
/// as required by AC-11.8 of the AI Copilot Suite PRD.
/// </summary>
public sealed class AIProviderException : Exception
{
    /// <summary>The provider that raised the error (e.g. "Bedrock", "OpenAI").</summary>
    public string ProviderName { get; }

    /// <summary>HTTP status code returned by the provider API, if applicable.</summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// Initialises a new instance with a descriptive message and originating provider.
    /// </summary>
    public AIProviderException(string providerName, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Initialises a new instance that also captures the HTTP status code for diagnostics.
    /// </summary>
    public AIProviderException(string providerName, int httpStatusCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ProviderName = providerName;
        HttpStatusCode = httpStatusCode;
    }
}
