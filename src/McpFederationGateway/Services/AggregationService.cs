using McpFederationGateway.Interfaces;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace McpFederationGateway.Services;

public class AggregationService : IAggregationService
{
    private readonly IConfigurationService _configService;
    private readonly IMcpClientFactory _clientFactory;

    public AggregationService(IConfigurationService configService, IMcpClientFactory clientFactory)
    {
        _configService = configService;
        _clientFactory = clientFactory;
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
                var toolsResult = await client.ListToolsAsync();
                var tools = toolsResult.Tools;
                
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
            catch (Exception)
            {
                // Log and continue
            }
        }

        // Add Federated Meta-Tools
        allTools.Add(new Tool
        {
            Name = "how_to_use",
            Description = "Provides documentation and usage for packages, libraries or MCP servers.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>(@"{
                ""type"": ""object"",
                ""properties"": {
                    ""package_or_library_or_mcp_server_name"": {
                        ""type"": ""string"",
                        ""description"": ""The name of the target to get documentation for""
                    }
                },
                ""required"": [""package_or_library_or_mcp_server_name""]
            }")
        });

        allTools.Add(new Tool
        {
            Name = "call",
            Description = "Calls a tool on a specific federated MCP server.",
            InputSchema = JsonSerializer.Deserialize<JsonElement>(@"{
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
            }")
        });

        return allTools;
    }
}
