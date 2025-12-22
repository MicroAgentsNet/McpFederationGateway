using ModelContextProtocol.Client;
using McpFederationGateway.Models;

namespace McpFederationGateway.Interfaces;

public interface IMcpClientFactory
{
    Task<IMcpClient> GetClientAsync(McpServerConfig config);
}
