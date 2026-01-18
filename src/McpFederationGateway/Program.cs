// Step 2: Add ConfigurationService
using McpFederationGateway.Interfaces;
using McpFederationGateway.Services;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add services
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IMcpClientFactory, McpClientFactory>();

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "GatewayWithConfig", Version = "1.0.0" };
})
.WithListToolsHandler(async (context, ct) =>
{
    Console.Error.WriteLine(">>> ListTools called, using ConfigurationService...");

    var configService = context.Services!.GetRequiredService<IConfigurationService>();
    var factory = context.Services!.GetRequiredService<IMcpClientFactory>();
    
    var config = await configService.GetConfigAsync();
    var repomixConfig = config.Servers.FirstOrDefault(s => s.Name == "repomix");
    
    if (repomixConfig == null)
    {
        Console.Error.WriteLine(">>> No repomix config found, using hardcoded");
        repomixConfig = new McpFederationGateway.Models.McpServerConfig
        {
            Name = "repomix",
            Transport = McpFederationGateway.Models.McpTransportType.Stdio,
            Command = "npx",
            Arguments = ["-y", "repomix", "--mcp"],
            Mode = McpFederationGateway.Models.McpOperationMode.Direct
        };
    }

    try
    {
        Console.Error.WriteLine($">>> Creating client for {repomixConfig.Name}...");
        var client = await factory.GetClientAsync(repomixConfig);
        Console.Error.WriteLine(">>> Client created!");

        var tools = await client.ListToolsAsync(cancellationToken: ct);
        Console.Error.WriteLine($">>> Got {tools.Count()} tools");

        return new ListToolsResult
        {
            Tools = tools.Select(t => new Tool
            {
                Name = $"repomix_{t.Name}",
                Description = t.Description
            }).ToList()
        };
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($">>> ERROR: {ex.Message}");
        throw;
    }
})
.WithStdioServerTransport();

await builder.Build().RunAsync();
