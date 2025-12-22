namespace McpFederationGateway.Models;

public class GatewayConfig
{
    public List<McpServerConfig> Servers { get; set; } = new();
}
