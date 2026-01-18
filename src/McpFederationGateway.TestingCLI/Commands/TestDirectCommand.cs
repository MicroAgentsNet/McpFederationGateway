/*
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using McpFederationGateway.Interfaces;
using McpFederationGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.TestingCLI.Commands;

/// <summary>
/// Tests the gateway by using its services directly (in-process) instead of via MCP protocol.
/// This avoids nested stdio deadlock issues.
/// </summary>
public class TestDirectCommand : Command
{
    private readonly Option<string> _serverOption;

    public TestDirectCommand() : base("test-direct", "Test gateway services directly (in-process, no nested stdio)")
    {
        _serverOption = new Option<string>("--server")
        {
            Description = "MCP server name to test",
            Required = true
        };

        Options.Add(_serverOption);
        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        var serverName = parseResult.GetValue(_serverOption)!;
        var logger = Program.ServiceProvider.GetRequiredService<ILogger<TestDirectCommand>>();

        try
        {
            logger.LogInformation("=== Test Direct Command ===");
            logger.LogInformation("Server: {ServerName}", serverName);

            Console.WriteLine($"\nTesting gateway services directly for server: {serverName}");
            Console.WriteLine(new string('-', 80));

            // Set the MICROAGENTS_CONFIG environment variable to point to the gateway config
            var configPath = Environment.GetEnvironmentVariable("GATEWAY_CONFIG")
                ?? Path.Combine(Environment.CurrentDirectory, ".microagents", "gateway-config.json");
            Environment.SetEnvironmentVariable("MICROAGENTS_CONFIG", configPath);

            // Create gateway services directly
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Trace;
                });
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Register gateway services
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IMcpClientFactory, McpClientFactory>();
            services.AddSingleton<IAggregationService, AggregationService>();
            services.AddScoped<IRouterService, RouterService>();
            services.AddScoped<IMicroAgentChatClient, SimpleMicroAgentChatClient>();

            var serviceProvider = services.BuildServiceProvider();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Test calling how_to_use directly
            Console.WriteLine("\n1. Testing how_to_use via RouterService directly...");
            var router = serviceProvider.GetRequiredService<IRouterService>();

            var args = new Dictionary<string, object?>
            {
                ["server_name"] = serverName,
                ["topic"] = "overview"
            };

            var result = await router.CallToolAsync("how_to_use", args, cts.Token);

            Console.WriteLine("Result:");
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            logger.LogInformation("Test completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test failed");
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }
}
*/
