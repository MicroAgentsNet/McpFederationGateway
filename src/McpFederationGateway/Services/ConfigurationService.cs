using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;

namespace McpFederationGateway.Services;

public class ConfigurationService : IConfigurationService
{
    public Task<GatewayConfig> GetConfigAsync()
    {
        // For demonstration, returning a dummy config.
        // In a real app, this would come from appsettings.json or a database.
        return Task.FromResult(new GatewayConfig
        {
            Servers = new List<McpServerConfig>
            {
                new McpServerConfig 
                { 
                    Name = "weather", 
                    Transport = "stdio", 
                    Command = "npx", 
                    Arguments = new[] { "-y", "@modelcontextprotocol/server-weather" } 
                }
            }
        });
    }
}
