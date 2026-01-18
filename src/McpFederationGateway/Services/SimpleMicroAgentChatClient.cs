using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.Services;

/// <summary>
/// Simple chat client implementation that returns tool summaries as-is without LLM generation.
/// Used for testing/debugging when sampling is not needed or not working.
/// </summary>
public class SimpleMicroAgentChatClient : IMicroAgentChatClient
{
    private readonly ILogger<SimpleMicroAgentChatClient> _logger;

    public SimpleMicroAgentChatClient(ILogger<SimpleMicroAgentChatClient> logger)
    {
        _logger = logger;
    }

    public Task<ChatResponse> GetResponseAsync(MicroAgentChatRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SimpleMicroAgentChatClient: Returning tool summary without LLM generation");

        // Extract the content from the last user message (which contains tool summaries)
        var lastUserMessage = request.Messages.LastOrDefault(m => m.Role == ChatRole.User);
        var responseText = lastUserMessage?.Content ?? "No tools available.";

        return Task.FromResult(new ChatResponse
        {
            Text = responseText,
            ModelId = "simple-passthrough",
            FinishReason = "passthrough"
        });
    }
}
