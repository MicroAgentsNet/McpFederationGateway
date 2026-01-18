using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using McpFederationGateway.TestingCLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.TestingCLI.Commands;

public class TestFederatedCommand : Command
{
    private readonly Option<string> _serverOption;
    private readonly Option<string?> _toolOption;

    public TestFederatedCommand() : base("test-federated", "Test federated mode (how_to_use and call meta-tools)")
    {
        _serverOption = new Option<string>("--server")
        {
            Description = "MCP server name (optional, if omitted performs global discovery)",
            Required = false
        };

        _toolOption = new Option<string?>("--tool")
        {
            Description = "Tool name to call (optional, will use how_to_use if not provided)"
        };

        Options.Add(_serverOption);
        Options.Add(_toolOption);

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        var serverName = parseResult.GetValue(_serverOption)!;
        var toolName = parseResult.GetValue(_toolOption);
        var logger = Program.ServiceProvider.GetRequiredService<ILogger<TestFederatedCommand>>();
        var gatewayClient = Program.ServiceProvider.GetRequiredService<McpGatewayClient>();

        try
        {
            logger.LogInformation("=== Test Federated Command ===");
            logger.LogInformation("Server: {ServerName}", serverName);

            Console.WriteLine($"\nTesting federated mode for server: {serverName}");
            Console.WriteLine(new string('-', 80));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Test how_to_use meta-tool
            Console.WriteLine("\n1. Testing 'how_to_use' meta-tool...");
            var howToUseArgs = new Dictionary<string, object?>();

            if (!string.IsNullOrEmpty(serverName))
            {
                howToUseArgs["server_name"] = serverName;
                howToUseArgs["topic"] = "overview";
            }
            
            var howToUseResult = await gatewayClient.CallToolAsync("how_to_use", howToUseArgs, cts.Token);
            Console.WriteLine("Result:");
            Console.WriteLine(JsonSerializer.Serialize(howToUseResult, new JsonSerializerOptions { WriteIndented = true }));

            // Test call meta-tool if tool name is provided
            if (!string.IsNullOrWhiteSpace(toolName))
            {
                if (string.IsNullOrEmpty(serverName))
                {
                    logger.LogError("Server name is required when calling a tool");
                    Console.WriteLine("\nError: Server name is required when calling a tool");
                    return 1;
                }

                Console.WriteLine($"\n2. Testing 'call' meta-tool with {toolName}...");
                var callArgs = new Dictionary<string, object?>
                {
                    ["server_name"] = serverName,
                    ["tool_name"] = toolName,
                    ["arguments"] = new Dictionary<string, object?>()
                };

                var callResult = await gatewayClient.CallToolAsync("call", callArgs, cts.Token);
                Console.WriteLine("Result:");
                Console.WriteLine(JsonSerializer.Serialize(callResult, new JsonSerializerOptions { WriteIndented = true }));
            }

            logger.LogInformation("Test completed successfully");
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test failed");
            Console.WriteLine($"\nError: {ex.Message}");
            return 1;
        }
    }
}
