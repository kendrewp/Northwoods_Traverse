namespace Traverse.AI.Abstractions.Models;

/// <summary>
/// Represents the role of a participant in a conversation with an AI model.
/// Mirrors the user/assistant/system taxonomy used by all supported providers.
/// </summary>
public enum AIMessageRole
{
    /// <summary>The platform or application providing context and instructions.</summary>
    System,

    /// <summary>The end user or caller driving the conversation.</summary>
    User,

    /// <summary>The AI model's prior response, included to maintain multi-turn context.</summary>
    Assistant,
}

/// <summary>
/// A single message in an AI conversation turn.
/// Immutable record — copy-with syntax is preferred over mutation.
/// </summary>
/// <param name="Role">Who authored this message.</param>
/// <param name="Content">The text content of the message.</param>
public sealed record AIMessage(AIMessageRole Role, string Content)
{
    /// <summary>Convenience factory for a system-role message.</summary>
    public static AIMessage System(string content) => new(AIMessageRole.System, content);

    /// <summary>Convenience factory for a user-role message.</summary>
    public static AIMessage User(string content) => new(AIMessageRole.User, content);

    /// <summary>Convenience factory for an assistant-role message.</summary>
    public static AIMessage Assistant(string content) => new(AIMessageRole.Assistant, content);
}
