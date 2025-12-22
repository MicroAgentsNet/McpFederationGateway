using McpFederationGateway.Interfaces;
using ModelContextProtocol.Protocol;
using McpFederationGateway.Models;

namespace McpFederationGateway.Services;

public class RouterService : IRouterService
{
    private readonly IConfigurationService _configService;
    private readonly IMcpClientFactory _clientFactory;
    private readonly IMicroAgentChatClient _chatClient;

    public RouterService(IConfigurationService configService, IMcpClientFactory clientFactory, IMicroAgentChatClient chatClient)
    {
        _configService = configService;
        _clientFactory = clientFactory;
        _chatClient = chatClient;
    }

    public async Task<CallToolResult> CallToolAsync(string fullName, IDictionary<string, object?> arguments, CancellationToken cancellationToken)
    {
        if (fullName == "call")
        {
            if (!arguments.TryGetValue("server_name", out var serverObj) || serverObj is not string serverName)
                throw new ArgumentException("Argument 'server_name' is required for 'call' tool");

            if (!arguments.TryGetValue("tool_name", out var toolObj) || toolObj is not string toolName)
                throw new ArgumentException("Argument 'tool_name' is required for 'call' tool");

            var actualArgs = arguments.TryGetValue("arguments", out var argsObj) && argsObj is IDictionary<string, object?> d ? d : new Dictionary<string, object?>();

             return await InvokeDirectAsync(serverName, toolName, actualArgs, cancellationToken);
        }

        if (fullName == "how_to_use")
        {
            if (!arguments.TryGetValue("mcp_server_name", out var targetObj) || targetObj is not string targetName)
                throw new ArgumentException("Argument 'mcp_server_name' is required for 'how_to_use' tool");

            var topic = arguments.TryGetValue("topic", out var topicObj) && topicObj is string t ? t : null;

            return await GenerateDocumentationAsync(targetName, topic, cancellationToken);
        }

        // Direct prefixed tool handling. Format: serverName_toolName
        var firstUnderscore = fullName.IndexOf('_');
        if (firstUnderscore > 0)
        {
            var serverName = fullName.Substring(0, firstUnderscore);
            var toolName = fullName.Substring(firstUnderscore + 1);
            return await InvokeDirectAsync(serverName, toolName, arguments, cancellationToken);
        }

        throw new KeyNotFoundException($"Tool {fullName} not found.");
    }

    private async Task<CallToolResult> InvokeDirectAsync(string serverName, string toolName, IDictionary<string, object?> arguments, CancellationToken cancellationToken)
    {
        var config = await _configService.GetConfigAsync();
        var serverConfig = config.Servers.FirstOrDefault(s => s.Name == serverName)
            ?? throw new KeyNotFoundException($"Server {serverName} not found in configuration.");

        var client = await _clientFactory.GetClientAsync(serverConfig);
        var readOnlyArgs = arguments as IReadOnlyDictionary<string, object?> ?? new Dictionary<string, object?>(arguments);
        return await client.CallToolAsync(toolName, readOnlyArgs);
    }

    private async Task<CallToolResult> GenerateDocumentationAsync(string serverName, string? topic, CancellationToken cancellationToken)
    {
        var config = await _configService.GetConfigAsync();
        var serverConfig = config.Servers.FirstOrDefault(s => s.Name == serverName)
            ?? throw new KeyNotFoundException($"Server {serverName} not found in configuration.");

        var client = await _clientFactory.GetClientAsync(serverConfig);
        var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);

        var toolSummaries = string.Join("\n", tools.Select(t => $"- {t.Name}: {t.Description}"));
        
        var prompt = $"""
            You are the MCP Federation Gateway. Provide a concise guide on how to use the MCP server '{serverName}'.
            
            Available tools in this server:
            {toolSummaries}
            
            {(topic != null ? $"Focus particularly on the topic: {topic}" : "")}
            
            Explain what this server does and give examples of when and how to call its tools.
            """;

        var response = await _chatClient.GetResponseAsync(new MicroAgentChatRequest
        {
            Messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, prompt) },
            Options = new ChatOptions { MaxOutputTokens = 1000 }
        }, cancellationToken);

        return new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = response.Text ?? "No documentation generated." }
            }
        };
    }
}
