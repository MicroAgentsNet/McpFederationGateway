using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using McpFederationGateway.Interfaces;
using McpFederationGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("MicroAgents MCP Federation Gateway CLI");

        var serverOption = new Option<string>("--server", "Server name to test")
        {
             DefaultValueFactory = _ => "repomix"
        };
        
        var testCommand = new Command("test", "Run direct gateway service tests");
        testCommand.Options.Add(serverOption);

        testCommand.SetAction(async parseResult =>
        {
            var serverName = parseResult.GetValue(serverOption) ?? "repomix";
            
            Console.WriteLine("=== MCP Federation Gateway Direct Test ===\n");
            Console.WriteLine($"Testing server: {serverName}\n");

            // Set config path
            var configPath = Environment.GetEnvironmentVariable("GATEWAY_CONFIG")
                ?? ".microagents/gateway-config.json";
            Environment.SetEnvironmentVariable("MICROAGENTS_CONFIG", configPath);

            // Setup DI
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Trace;
                });
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IMcpClientFactory, McpClientFactory>();
            services.AddSingleton<IAggregationService, AggregationService>();
            services.AddScoped<IRouterService, RouterService>();
            services.AddScoped<IMicroAgentChatClient, SimpleMicroAgentChatClient>();

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                Console.WriteLine("Calling RouterService.CallToolAsync...");
                var router = serviceProvider.GetRequiredService<IRouterService>();

                var toolArgs = new Dictionary<string, object?>
                {
                    ["server_name"] = serverName,
                    ["topic"] = "overview"
                };

                var result = await router.CallToolAsync("how_to_use", toolArgs, cts.Token);

                Console.WriteLine("\nResult:");
                Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

                Console.WriteLine("\nTest completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        });

        var adminCommand = new Command("admin", "Administrative commands");
        // Future implementation

        rootCommand.Subcommands.Add(testCommand);
        rootCommand.Subcommands.Add(adminCommand);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
