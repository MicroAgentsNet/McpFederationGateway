using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using McpFederationGateway.TestingCLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.TestingCLI.Commands;

public class TestCallCommand : Command
{
    private readonly Option<string> _toolOption;
    private readonly Option<string> _argsOption;

    public TestCallCommand() : base("test-call", "Test direct tool invocation")
    {
        _toolOption = new Option<string>("--tool")
        {
            Description = "Tool name to invoke",
            Required = true
        };

        _argsOption = new Option<string>("--args")
        {
            Description = "JSON arguments for the tool (optional)",
            DefaultValueFactory = _ => "{}"
        };

        Options.Add(_toolOption);
        Options.Add(_argsOption);

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        var toolName = parseResult.GetValue(_toolOption)!;
        var argsJson = parseResult.GetValue(_argsOption)!;
        var logger = Program.ServiceProvider.GetRequiredService<ILogger<TestCallCommand>>();
        var gatewayClient = Program.ServiceProvider.GetRequiredService<McpGatewayClient>();

        try
        {
            logger.LogInformation("=== Test Call Command ===");
            logger.LogInformation("Tool: {ToolName}", toolName);
            logger.LogInformation("Arguments JSON: {Args}", argsJson);

            var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(argsJson) ?? new Dictionary<string, object?>();

            Console.WriteLine($"\nCalling tool: {toolName}");
            Console.WriteLine($"Arguments: {argsJson}");
            Console.WriteLine(new string('-', 80));

            var startTime = DateTime.UtcNow;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var result = await gatewayClient.CallToolAsync(toolName, arguments, cts.Token);
            var elapsed = DateTime.UtcNow - startTime;

            Console.WriteLine($"\nResponse received in {elapsed.TotalMilliseconds:F2}ms");
            Console.WriteLine("Result:");
            Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            logger.LogInformation("Test completed successfully. Latency: {Latency}ms", elapsed.TotalMilliseconds);
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
