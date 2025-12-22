using McpFederationGateway;
using McpFederationGateway.Interfaces;
using McpFederationGateway.Services;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceGenerationContext.Default);
});

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Register custom services
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IMcpClientFactory, McpClientFactory>();
builder.Services.AddSingleton<IAggregationService, AggregationService>();

// Chat and Routing (Scoped because they depend on the session's McpServer for Sampling)
builder.Services.AddScoped<IMicroAgentChatClient, SamplingMicroAgentChatClient>();
builder.Services.AddScoped<IRouterService, RouterService>();

var mcpBuilder = builder.Services.AddMcpServer(options => 
{
    options.ServerInfo = new Implementation 
    { 
        Name = "McpFederationGateway", 
        Version = "1.0.0" 
    };
})
.WithListToolsHandler(async (context, ct) =>
{
    var aggregator = context.Services.GetRequiredService<IAggregationService>();
    var tools = await aggregator.GetToolsAsync();
    return new ListToolsResult { Tools = tools.ToList() };
})
.WithCallToolHandler(async (context, ct) =>
{
    var router = context.Services.GetRequiredService<IRouterService>();
    var request = context.Params;
    var args = request.Arguments?.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value) ?? new Dictionary<string, object?>();
    return await router.CallToolAsync(request.Name, args, ct);
});

bool isStdio = args.Contains("--transport") && args[args.ToList().IndexOf("--transport") + 1].ToLower() == "stdio";

if (isStdio)
{
    mcpBuilder.WithStdioServerTransport();
}
else
{
    mcpBuilder.WithHttpTransport();
}

var app = builder.Build();

if (!isStdio)
{
    app.MapMcp();
}

await app.RunAsync();
