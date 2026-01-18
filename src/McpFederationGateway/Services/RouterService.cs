using McpFederationGateway.Interfaces;
using ModelContextProtocol.Protocol;
using McpFederationGateway.Models;
using McpFederationGateway.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace McpFederationGateway.Services;

public class RouterService : IRouterService
{
    private readonly IConfigurationService _configService;
    private readonly IMcpClientFactory _clientFactory;
    private readonly IMicroAgentChatClient _chatClient;
    private readonly ILogger<RouterService> _logger;

    public RouterService(IConfigurationService configService, IMcpClientFactory clientFactory, IMicroAgentChatClient chatClient, ILogger<RouterService> logger)
    {
        _configService = configService;
        _clientFactory = clientFactory;
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<CallToolResult> CallToolAsync(string fullName, IDictionary<string, object?>? arguments, CancellationToken cancellationToken)
    {
        var validArgs = arguments ?? new Dictionary<string, object?>();
        _logger.LogInformation("RouterService received CallTool: {ToolName}", fullName);
        
        foreach (var kvp in validArgs)
        {
            _logger.LogInformation("Arg: {Key}, Type: {Type}, Value: {Value}", kvp.Key, kvp.Value?.GetType().Name ?? "null", kvp.Value);
        }

        if (fullName == "call")
        {
            if (!McpUtility.TryGetString(validArgs, "server_name", out var serverName) || string.IsNullOrEmpty(serverName))
                throw new ArgumentException("Argument 'server_name' is required for 'call' tool");

            if (!McpUtility.TryGetString(validArgs, "tool_name", out var toolName) || string.IsNullOrEmpty(toolName))
                throw new ArgumentException("Argument 'tool_name' is required for 'call' tool");

            var actualArgs = McpUtility.TryGetDictionary(validArgs, "arguments", out var args)
                ? args
                : new Dictionary<string, object?>();

             return await InvokeDirectAsync(serverName, toolName, actualArgs, cancellationToken);
        }

        if (fullName == "how_to_use")
        {
            McpUtility.TryGetString(validArgs, "server_name", out var targetName);
            McpUtility.TryGetString(validArgs, "topic", out var topic);

            if (string.IsNullOrEmpty(targetName))
            {
                return await GenerateGlobalDocumentationAsync(cancellationToken);
            }

            // Use new method without sampling
            return await GenerateDocumentationWithoutSamplingAsync(targetName, topic, cancellationToken);
        }

        // Direct prefixed tool handling. Format: serverName_toolName
        var firstUnderscore = fullName.IndexOf('_');
        if (firstUnderscore > 0)
        {
            var serverName = fullName.Substring(0, firstUnderscore);
            var toolName = fullName.Substring(firstUnderscore + 1);
            return await InvokeDirectAsync(serverName, toolName, validArgs, cancellationToken);
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

    // Original method using sampling (kept for reference)
    private async Task<CallToolResult> GenerateDocumentationAsync(string serverName, string? topic, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">>> GenerateDocumentationAsync START for {ServerName}", serverName);
        var config = await _configService.GetConfigAsync();
        var serverConfig = config.Servers.FirstOrDefault(s => s.Name == serverName)
            ?? throw new KeyNotFoundException($"Server {serverName} not found in configuration.");

        _logger.LogInformation(">>> Calling GetClientAsync for {ServerName}", serverName);
        var client = await _clientFactory.GetClientAsync(serverConfig);
        _logger.LogInformation(">>> GetClientAsync completed for {ServerName}", serverName);

        _logger.LogInformation(">>> Calling ListToolsAsync for {ServerName}", serverName);
        var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);
        _logger.LogInformation(">>> ListToolsAsync completed for {ServerName}, found {ToolCount} tools", serverName, tools.Count());

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

    // New method that returns content as-is without sampling
    private async Task<CallToolResult> GenerateDocumentationWithoutSamplingAsync(string serverName, string? topic, CancellationToken cancellationToken)
    {
        _logger.LogInformation(">>> GenerateDocumentationWithoutSamplingAsync START for {ServerName}", serverName);
        var config = await _configService.GetConfigAsync();
        var serverConfig = config.Servers.FirstOrDefault(s => s.Name == serverName)
            ?? throw new KeyNotFoundException($"Server {serverName} not found in configuration.");

        _logger.LogInformation(">>> Calling GetClientAsync for {ServerName}", serverName);
        var client = await _clientFactory.GetClientAsync(serverConfig);
        _logger.LogInformation(">>> GetClientAsync completed for {ServerName}", serverName);

        _logger.LogInformation(">>> Calling ListToolsAsync for {ServerName}", serverName);
        var tools = await client.ListToolsAsync(cancellationToken: cancellationToken);
        _logger.LogInformation(">>> ListToolsAsync completed for {ServerName}, found {ToolCount} tools", serverName, tools.Count());

        var toolSummaries = string.Join("\n", tools.Select(t => $"- {t.Name}: {t.Description}"));

        var documentation = $"""
            MCP Server: {serverName}

            Available tools:
            {toolSummaries}

            {(topic != null ? $"\nFocused on topic: {topic}" : "")}

            To use these tools, call them through the gateway using the 'call' tool or the prefixed format '{serverName}_toolName'.
            """;

        return new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = documentation }
            }
        };
    }

    private async Task<CallToolResult> GenerateGlobalDocumentationAsync(CancellationToken cancellationToken)
    {
        var config = await _configService.GetConfigAsync();
        var federatedServers = config.Servers
            .Where(s => s.Mode == McpOperationMode.Federated)
            .Select(s => s.Name)
            .ToList();

        var serverList = string.Join(", ", federatedServers.Select(s => $"'{s}'"));

        return new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock 
                { 
                    Text = $"Global Discovery: This gateway federates multiple MCP servers. Available federated servers are: {serverList}. " +
                           "Use 'how_to_use' with 'server_name' parameter set to one of these names to get detailed documentation."
                }
            }
        };
    }
}
