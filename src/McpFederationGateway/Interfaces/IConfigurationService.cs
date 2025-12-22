using McpFederationGateway.Models;

namespace McpFederationGateway.Interfaces;

public interface IConfigurationService
{
    Task<GatewayConfig> GetConfigAsync();
}
