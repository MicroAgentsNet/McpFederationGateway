using McpFederationGateway.Interfaces;
using McpFederationGateway.Services;
using ModelContextProtocol.Server;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Configure Serilog
try
{
    var builder = Host.CreateApplicationBuilder(args);

    // Clear default providers and add Serilog configuration
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger());
    
    // Register Services
    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddSingleton<IMcpClientFactory, McpClientFactory>();
    builder.Services.AddSingleton<IAggregationService, AggregationService>();
    builder.Services.AddSingleton<IMicroAgentChatClient, SamplingMicroAgentChatClient>();
    builder.Services.AddSingleton<IRouterService, RouterService>();

    // Register MCP Server
    builder.Services.AddMcpServer(options =>
    {
        options.ServerInfo = new ModelContextProtocol.Protocol.Implementation { Name = "McpFederationGateway", Version = "1.0.0" };
        options.Capabilities = new ModelContextProtocol.Protocol.ServerCapabilities
        {
            Tools = new ModelContextProtocol.Protocol.ToolsCapability { ListChanged = true },
            Prompts = new ModelContextProtocol.Protocol.PromptsCapability { ListChanged = true },
            Resources = new ModelContextProtocol.Protocol.ResourcesCapability { ListChanged = true, Subscribe = true }
        };
    })
    .WithListToolsHandler(async (request, ct) =>
    {
        var aggregationService = request.Services!.GetRequiredService<IAggregationService>();
        var tools = await aggregationService.GetToolsAsync();
        return new ModelContextProtocol.Protocol.ListToolsResult { Tools = tools.ToList() };
    })
    .WithCallToolHandler(async (request, ct) =>
    {
        var routerService = request.Services!.GetRequiredService<IRouterService>();
        var params_ = request.Params!;
        var args = params_.Arguments?.ToDictionary(k => k.Key, v => (object?)v.Value) 
                   ?? new Dictionary<string, object?>();
        return await routerService.CallToolAsync(params_.Name, args, ct);
    })
    .WithStdioServerTransport();

    var app = builder.Build();
    
    // Pre-initialize clients (fire and forget, or wait?)
    // While developing, it's safer to let them initialize on demand or explicit background task.
    // But for now, let's just run.

    await app.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Application start-up failed: {ex}");
}
