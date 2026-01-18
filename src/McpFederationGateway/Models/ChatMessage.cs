using System.Diagnostics.CodeAnalysis;

namespace McpFederationGateway.Models;

/// <summary>
/// Represents a message in a chat conversation.
/// </summary>
public record ChatMessage
{
    /// <summary>
    /// Creates a new ChatMessage with User role by default.
    /// </summary>
    [SetsRequiredMembers]
    public ChatMessage()
    {
        Role = ChatRole.User;
        Content = null;
    }

    /// <summary>
    /// Creates a new ChatMessage with the specified role and content.
    /// </summary>
    /// <param name="role">The role of the message sender.</param>
    /// <param name="content">The text content of the message.</param>
    [SetsRequiredMembers]
    public ChatMessage(ChatRole role, string? content)
    {
        Role = role;
        Content = content;
    }

    /// <summary>
    /// The role of the message sender (System, User, or Assistant).
    /// </summary>
    public required ChatRole Role { get; init; }

    /// <summary>
    /// The text content of the message.
    /// </summary>
    public required string? Content { get; init; }

    /// <summary>
    /// Optional name of the sender (for multi-user chats).
    /// </summary>
    public string? Name { get; init; }
}
