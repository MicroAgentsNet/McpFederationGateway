using McpFederationGateway.Interfaces;
using ModelContextProtocol.Protocol;

namespace McpFederationGateway.Services;

public class RouterService : IRouterService
{
    private readonly IConfigurationService _configService;
    private readonly IMcpClientFactory _clientFactory;

    public RouterService(IConfigurationService configService, IMcpClientFactory clientFactory)
    {
        _configService = configService;
        _clientFactory = clientFactory;
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

            // TODO: Implement LLM-based documentation generation as per ARCHITECTURE.md
            return new CallToolResult 
            { 
                 Content = new List<ContentBlock> 
                 { 
                     new TextContentBlock { Text = $"Documentation for '{targetName}' is not yet implemented using host LLM." } 
                 } 
            };
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
        return await client.CallToolAsync(toolName, arguments, cancellationToken);
    }
}
