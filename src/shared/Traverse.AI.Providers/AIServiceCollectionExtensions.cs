using Amazon.BedrockRuntime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Traverse.AI.Abstractions;
using Traverse.AI.Providers.Anthropic;
using Traverse.AI.Providers.Bedrock;
using Traverse.AI.Providers.OpenAI;
using Traverse.AI.Providers.Options;

namespace Traverse.AI.Providers;

/// <summary>
/// Extension methods that register the AI provider abstraction into an
/// <see cref="IServiceCollection"/> using the Options pattern.
///
/// <para>
/// Call <see cref="AddTraverseAI"/> in each microservice's <c>Program.cs</c> or
/// service-registration extension method.  The active provider is resolved at startup
/// from <c>AI:Provider</c> in configuration, so switching providers requires only a
/// configuration change — no code change.
/// </para>
/// </summary>
public static class AIServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IAIProvider"/> as a singleton, selecting the implementation
    /// indicated by <c>AI:Provider</c> in <paramref name="configuration"/>.
    ///
    /// <para><b>Usage:</b></para>
    /// <code>
    /// builder.Services.AddTraverseAI(builder.Configuration);
    /// </code>
    ///
    /// <para><b>Bedrock credential chain (no key in config):</b> When
    /// <see cref="AIProviderType.Bedrock"/> is active, an <see cref="AmazonBedrockRuntimeClient"/>
    /// is registered using the AWS SDK default credential chain
    /// (IAM instance role → environment variables → ~/.aws/credentials).</para>
    ///
    /// <para><b>API keys (OpenAI / Anthropic):</b> Must be supplied via
    /// <c>dotnet user-secrets</c> in development and Kubernetes Secrets or
    /// Azure Key Vault in production.  They must never appear in checked-in
    /// appsettings files.</para>
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">Application configuration (IConfiguration).</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddTraverseAI(
        this IServiceCollection services,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        // Bind and validate options at startup so a missing/invalid config key surfaces
        // immediately rather than silently failing at the first AI call.
        services
            .AddOptions<AIOptions>()
            .BindConfiguration(AIOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the concrete provider selected by configuration.
        // Using a factory delegate keeps the registration lazy while still resolving
        // to a singleton, which avoids creating unused provider clients.
        services.AddSingleton<IAIProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<AIOptions>>().Value;

            return options.Provider switch
            {
                AIProviderType.Bedrock    => CreateBedrockProvider(sp),
                AIProviderType.OpenAI     => ActivatorUtilities.CreateInstance<OpenAIAIProvider>(sp),
                AIProviderType.Anthropic  => ActivatorUtilities.CreateInstance<AnthropicAIProvider>(sp),
                _ => throw new InvalidOperationException(
                    $"Unsupported AI provider '{options.Provider}'. " +
                    $"Valid values: {string.Join(", ", Enum.GetNames<AIProviderType>())}"),
            };
        });

        return services;
    }

    /// <summary>
    /// Creates a <see cref="BedrockAIProvider"/> and registers the underlying
    /// <see cref="IAmazonBedrockRuntime"/> client into the container so it can be
    /// replaced with a test double in integration tests.
    /// </summary>
    private static IAIProvider CreateBedrockProvider(IServiceProvider sp)
    {
        // Register AmazonBedrockRuntimeClient as singleton when Bedrock is active.
        // The client is thread-safe and expensive to construct; a single instance is reused.
        var options = sp.GetRequiredService<IOptions<AIOptions>>().Value;
        var bedrockClient = new AmazonBedrockRuntimeClient(
            Amazon.RegionEndpoint.GetBySystemName(options.Bedrock.Region));

        // Wrap in a factory so the test suite can inject IAmazonBedrockRuntime directly
        // via sp.GetRequiredService<IAmazonBedrockRuntime>() if it was pre-registered.
        var runtimeClient = sp.GetService<IAmazonBedrockRuntime>() ?? bedrockClient;

        return ActivatorUtilities.CreateInstance<BedrockAIProvider>(sp, runtimeClient);
    }
}
