using McpFederationGateway.Models;
namespace McpFederationGateway.Interfaces;

/// <summary>
/// Unified interface for chat/LLM interactions.
/// </summary>
public interface IMicroAgentChatClient
{
    Task<ChatResponse> GetResponseAsync(MicroAgentChatRequest request, CancellationToken cancellationToken = default);
}
