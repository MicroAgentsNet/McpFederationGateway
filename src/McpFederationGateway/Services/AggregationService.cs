using McpFederationGateway.Interfaces;
using ModelContextProtocol.Protocol;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using McpFederationGateway.Models;

namespace McpFederationGateway.Services;

public class AggregationService : IAggregationService
{
    private readonly IConfigurationService _configService;
    private readonly IMcpClientFactory _clientFactory;
    private readonly ILogger<AggregationService> _logger;

    public AggregationService(IConfigurationService configService, IMcpClientFactory clientFactory, ILogger<AggregationService> logger)
    {
        _configService = configService;
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<Tool>> GetToolsAsync()
    {
        var config = await _configService.GetConfigAsync();
        var allTools = new List<Tool>();

        foreach (var serverConfig in config.Servers)
        {
            var serverName = serverConfig.Name;
            try
            {
                if (serverConfig.Mode.ToLower() == "federated")
                {
                    continue;
                }

                var client = await _clientFactory.GetClientAsync(serverConfig);
                var tools = await client.ListToolsAsync();
                
                foreach (var mcpTool in tools)
                {
                    var tool = mcpTool.ProtocolTool;
                    allTools.Add(new Tool
                    {
                        Name = $"{serverName}_{tool.Name}",
                        Description = $"[{serverName}] {tool.Description}",
                        InputSchema = tool.InputSchema
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to aggregate tools from server {ServerName}", serverName);
            }
        }

        // Add Federated Meta-Tools
        allTools.Add(new Tool
        {
            Name = "how_to_use",
            Description = "Provides documentation, summaries, and usage guides for a specific federated MCP server.",
            InputSchema = JsonSerializer.Deserialize(@"{
                ""type"": ""object"",
                ""properties"": {
                    ""mcp_server_name"": {
                        ""type"": ""string"",
                        ""description"": ""The name of the federated MCP server to get documentation for""
                    }
                },
                ""required"": [""mcp_server_name""]
            }", AppJsonContext.Default.JsonElement)
        });

        allTools.Add(new Tool
        {
            Name = "call",
            Description = "Calls a tool on a specific federated MCP server.",
            InputSchema = JsonSerializer.Deserialize(@"{
                ""type"": ""object"",
                ""properties"": {
                    ""server_name"": {
                        ""type"": ""string"",
                        ""description"": ""The federated server name""
                    },
                    ""tool_name"": {
                        ""type"": ""string"",
                        ""description"": ""The target tool name on the server""
                    },
                    ""arguments"": {
                        ""type"": ""object"",
                        ""description"": ""Arguments for the tool""
                    }
                },
                ""required"": [""server_name"", ""tool_name""]
            }", AppJsonContext.Default.JsonElement)
        });

        return allTools;
    }
}
