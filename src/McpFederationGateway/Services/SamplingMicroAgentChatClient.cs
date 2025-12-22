using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;
using ModelContextProtocol.Server;

namespace McpFederationGateway.Services;

/// <summary>
/// Chat client implementation using MCP Server's sampling capability.
/// TODO: Implement this properly with MCP sampling API once the LLM provider is configured.
/// </summary>
public class SamplingMicroAgentChatClient : IMicroAgentChatClient
{
    private readonly McpServer _server;

    public SamplingMicroAgentChatClient(McpServer server)
    {
        _server = server;
    }

    public Task<ChatResponse> GetResponseAsync(MicroAgentChatRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: Implement actual MCP sampling once LLM provider is configured
        // This will require:
        // 1. Setting up an LLM provider (OpenAI, Anthropic, etc.) in Program.cs
        // 2. Configuring the MCP server's sampling handler
        // 3. Converting our ChatMessage/ChatOptions to MCP protocol types
        // 4. Calling the sampling handler and mapping results back
        
        throw new NotImplementedException(
            "LLM-based documentation generation is not yet configured. " +
            "The how_to_use meta-tool requires an LLM provider to be set up in Program.cs.");
    }
}
