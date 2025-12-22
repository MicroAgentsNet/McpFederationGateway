using ModelContextProtocol.Client;
using McpFederationGateway.Models;

namespace McpFederationGateway.Interfaces;

public interface IMcpClientFactory
{
    Task<McpClient> GetClientAsync(McpServerConfig config);
}
