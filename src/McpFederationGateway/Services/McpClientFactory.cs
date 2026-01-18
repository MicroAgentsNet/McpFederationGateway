using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using McpFederationGateway.Interfaces;
using McpFederationGateway.Models;
using Microsoft.Extensions.Logging;


namespace McpFederationGateway.Services;

public class McpClientFactory : IMcpClientFactory
{
    private readonly Dictionary<string, McpClient> _clients = new();
    private readonly ILogger<McpClientFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public McpClientFactory(ILogger<McpClientFactory> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    public async Task<McpClient> GetClientAsync(McpServerConfig config)
    {
        _logger.LogInformation("GetClientAsync called for server: {ServerName}, Transport: {Transport}", config.Name, config.Transport);

        if (_clients.TryGetValue(config.Name, out var client))
        {
            _logger.LogDebug("Returning cached client for {ServerName}", config.Name);
            return client;
        }

        IClientTransport transport = config.Transport switch
        {
            McpTransportType.Stdio => new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = config.Name,
                Command = config.Command!,
                Arguments = config.Arguments ?? Array.Empty<string>(),
                EnvironmentVariables = config.Env?.ToDictionary(k => k.Key, v => (string?)v.Value),
                StandardErrorLines = line => _logger.LogWarning("Server {ServerName} stderr: {Line}", config.Name, line)
            }, _loggerFactory),
            McpTransportType.Http => new HttpClientTransport(new HttpClientTransportOptions
            {
                Name = config.Name,
                Endpoint = new Uri(config.Url!)
            }),
            _ => throw new NotSupportedException($"Transport {config.Transport} is not supported.")
        };

        if (config.Env != null)
        {
            _logger.LogInformation("Environment variables for {ServerName}: {EnvVars}", 
                config.Name, string.Join(", ", config.Env.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        _logger.LogInformation("Creating new client for {ServerName}...", config.Name);
        
        var mcpClient = await McpClient.CreateAsync(transport, new McpClientOptions 
        { 
            ClientInfo = new Implementation { Name = "McpFederationGateway", Version = "1.0.0" }
        });

        _logger.LogInformation("Client created (handshake complete) for {ServerName}", config.Name);
        _clients[config.Name] = mcpClient;
        return mcpClient;
    }
}
