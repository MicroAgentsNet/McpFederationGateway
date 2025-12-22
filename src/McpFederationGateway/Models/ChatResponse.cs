namespace McpFederationGateway.Models;

/// <summary>
/// Response from a chat/LLM invocation.
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// The text content of the response.
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// The model that generated the response.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// The finish reason (e.g., "stop", "length", "tool_calls").
    /// </summary>
    public string? FinishReason { get; set; }
}
