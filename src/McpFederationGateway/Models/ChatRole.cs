namespace McpFederationGateway.Models;

/// <summary>
/// The role of the message in a chat conversation.
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// System message providing instructions or context.
    /// </summary>
    System,
    
    /// <summary>
    /// Message from the user/human.
    /// </summary>
    User,
    
    /// <summary>
    /// Message from the AI assistant.
    /// </summary>
    Assistant
}
