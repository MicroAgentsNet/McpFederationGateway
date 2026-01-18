using System.CommandLine;
using System.CommandLine.Parsing;
using McpFederationGateway.TestingCLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.TestingCLI.Commands;

public class TestToolsCommand : Command
{
    public TestToolsCommand() : base("test-tools", "List all available tools from configured MCPs")
    {
        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        var logger = Program.ServiceProvider.GetRequiredService<ILogger<TestToolsCommand>>();
        var gatewayClient = Program.ServiceProvider.GetRequiredService<McpGatewayClient>();

        try
        {
            logger.LogInformation("=== Test Tools Command ===");
            logger.LogInformation("Listing all available tools...");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var tools = await gatewayClient.ListToolsAsync(cts.Token);
            var toolsList = tools.ToList();

            Console.WriteLine($"\nFound {toolsList.Count} tools:");
            Console.WriteLine(new string('-', 80));

            foreach (var tool in toolsList)
            {
                Console.WriteLine($"Tool: {tool.Name}");
                Console.WriteLine($"  Description: {tool.Description}");
                Console.WriteLine();
            }

            logger.LogInformation("Test completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test failed");
            return 1;
        }
    }
}
