using ModelContextProtocol.Client;
using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;
using ModelContextProtocol.Transport;

namespace McpFederationGateway.Services;

public class McpClientFactory : IMcpClientFactory
{
    private readonly Dictionary<string, IMcpClient> _clients = new();

    public async Task<IMcpClient> GetClientAsync(McpServerConfig config)
    {
        if (_clients.TryGetValue(config.Name, out var client))
        {
            return client;
        }

        IMcpTransport transport = config.Transport.ToLower() switch
        {
            "stdio" => new StdioClientTransport(config.Command!, config.Arguments ?? Array.Empty<string>()),
            "http" => new HttpTransport(config.Url!),
            _ => throw new NotSupportedException($"Transport {config.Transport} is not supported.")
        };

        var mcpClient = new McpClient(transport);
        await mcpClient.ConnectAsync();
        _clients[config.Name] = mcpClient;
        return mcpClient;
    }
}
