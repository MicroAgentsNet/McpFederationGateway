using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.TestingCLI.Commands;

public class TestConfigCommand : Command
{
    public TestConfigCommand() : base("test-config", "Validate MCP client configuration")
    {
        SetAction(ExecuteAsync);
    }

    private Task<int> ExecuteAsync(ParseResult parseResult)
    {
        var logger = Program.ServiceProvider.GetRequiredService<ILogger<TestConfigCommand>>();

        try
        {
            logger.LogInformation("=== Test Config Command ===");

            Console.WriteLine("\nMCP Gateway Configuration Test");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine("\nNote: When using MCP protocol to communicate with the gateway,");
            Console.WriteLine("configuration details are not exposed to the client.");
            Console.WriteLine("\nThe gateway's internal configuration is managed independently");
            Console.WriteLine("and only tool/call operations are exposed through the MCP protocol.");
            Console.WriteLine("\nTo verify the gateway is configured correctly:");
            Console.WriteLine("1. Use 'test-tools' to list available tools");
            Console.WriteLine("2. Use 'test-call' to invoke specific tools");
            Console.WriteLine("3. Check the gateway's own logs for configuration details");
            Console.WriteLine(new string('=', 80));

            logger.LogInformation("Config test completed - configuration is managed by gateway");
            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test failed");
            Console.WriteLine($"\nError: {ex.Message}");
            return Task.FromResult(1);
        }
    }
}
