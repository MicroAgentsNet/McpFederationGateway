namespace McpFederationGateway.Models;

/// <summary>
/// Options for controlling chat/LLM request behavior.
/// Based on LLMTornado's ChatRequest design but simplified for our needs.
/// </summary>
public class ChatOptions
{
    /// <summary>
    /// Maximum number of tokens to generate in the response.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// Temperature for randomness in responses (0.0 to 2.0).
    /// Lower values make output more focused and deterministic.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Model ID to use for the request (e.g., "gpt-4", "claude-3-sonnet").
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Top P sampling parameter (0.0 to 1.0).
    /// Controls diversity via nucleus sampling.
    /// </summary>
    public float? TopP { get; set; }
}
