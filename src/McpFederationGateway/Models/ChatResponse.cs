namespace McpFederationGateway.Models;

/// <summary>
/// Response from a chat/LLM invocation.
/// </summary>
public record ChatResponse
{
    /// <summary>
    /// The text content of the response.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// The model that generated the response.
    /// </summary>
    public string? ModelId { get; init; }

    /// <summary>
    /// The finish reason (e.g., "stop", "length", "tool_calls").
    /// </summary>
    public string? FinishReason { get; init; }
}
