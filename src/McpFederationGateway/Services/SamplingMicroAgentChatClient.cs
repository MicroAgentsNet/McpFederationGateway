using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ModelContextProtocol.Protocol;

namespace McpFederationGateway.Services;

/// <summary>
/// Chat client implementation using MCP Server's sampling capability.
/// Delegates the LLM generation to the connected MCP client (e.g., Claude Desktop, IDE).
/// </summary>
public class SamplingMicroAgentChatClient : IMicroAgentChatClient
{
    private readonly McpServer _server;
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<SamplingMicroAgentChatClient> _logger;

    public SamplingMicroAgentChatClient(
        McpServer server, 
        IConfigurationService configurationService, 
        ILogger<SamplingMicroAgentChatClient> logger)
    {
        _server = server;
        _configurationService = configurationService;
        _logger = logger;
    }

    public async Task<ChatResponse> GetResponseAsync(MicroAgentChatRequest request, CancellationToken cancellationToken = default)
    {
        var config = await _configurationService.GetConfigAsync();
        
        var samplingMessages = new List<SamplingMessage>();
        string? systemPrompt = null;

        foreach (var message in request.Messages)
        {
            if (message.Role == ChatRole.System)
            {
                if (systemPrompt == null)
                    systemPrompt = message.Content;
                else
                    systemPrompt += "\n" + message.Content;
            }
            else
            {
                var role = message.Role switch
                {
                    ChatRole.User => Role.User,
                    ChatRole.Assistant => Role.Assistant,
                    _ => Role.User
                };

                var content = new TextContentBlock
                {
                    Text = message.Content ?? string.Empty
                };

                samplingMessages.Add(new SamplingMessage
                {
                    Role = role,
                    Content = new List<ContentBlock> { content }
                });
            }
        }

        var modelPreferences = new ModelPreferences
        {
            CostPriority = 0.5f, 
            SpeedPriority = 0.5f,
            IntelligencePriority = 0.5f
        };

        if (config.Llm?.DefaultModel != null)
        {
            modelPreferences.Hints = new List<ModelHint> { new ModelHint { Name = config.Llm.DefaultModel } };
        }
        
        if (!string.IsNullOrEmpty(request.Options?.ModelId))
        {
             modelPreferences.Hints = new List<ModelHint> { new ModelHint { Name = request.Options.ModelId } };
        }

        var createMessageRequest = new CreateMessageRequestParams
        {
            Messages = samplingMessages.ToArray(),
            SystemPrompt = systemPrompt,
            IncludeContext = ContextInclusion.ThisServer, 
            Temperature = (float?)((request.Options?.Temperature ?? config.Llm?.Temperature)),
            MaxTokens = (int)(request.Options?.MaxOutputTokens ?? 4096),
            ModelPreferences = modelPreferences
        };

        try
        {
            _logger.LogInformation("Requesting sampling from client...");
            
            var result = await _server.SampleAsync(createMessageRequest, cancellationToken);
            
            var sb = new System.Text.StringBuilder();
            if (result.Content != null)
            {
                foreach (var block in result.Content)
                {
                    if (block is TextContentBlock textBlock)
                    {
                        sb.Append(textBlock.Text);
                    }
                }
            }
            
            var responseText = sb.ToString();

            return new ChatResponse
            {
                Text = responseText,
                ModelId = result.Model,
                FinishReason = result.StopReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sample message from client.");
            throw new InvalidOperationException("Failed to get response from MCP Client via Sampling.", ex);
        }
    }
}
