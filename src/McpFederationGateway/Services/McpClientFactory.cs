using ModelContextProtocol.Client;
using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;


namespace McpFederationGateway.Services;

public class McpClientFactory : IMcpClientFactory
{
    private readonly Dictionary<string, McpClient> _clients = new();

    public async Task<McpClient> GetClientAsync(McpServerConfig config)
    {
        if (_clients.TryGetValue(config.Name, out var client))
        {
            return client;
        }

        IClientTransport transport = config.Transport.ToLower() switch
        {
            "stdio" => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = config.Name,
                Command = config.Command!,
                Arguments = config.Arguments ?? Array.Empty<string>()
            }),
            "http" => new HttpClientTransport(new HttpClientTransportOptions
            {
                Name = config.Name,
                Endpoint = new Uri(config.Url!)
            }),
            _ => throw new NotSupportedException($"Transport {config.Transport} is not supported.")
        };

        var mcpClient = await McpClient.CreateAsync(transport);
        _clients[config.Name] = mcpClient;
        return mcpClient;
    }
}
