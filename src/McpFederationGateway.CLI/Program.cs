using System.CommandLine;
using System.CommandLine.Parsing;

namespace McpFederationGateway.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("MicroAgents MCP Federation Gateway CLI");

        var testCommand = new Command("test", "Run diagnostics and tests");
        testCommand.SetAction((parseResult, cancellationToken) =>
        {
            Console.WriteLine("Running tests...");
            // Implementation or calling test logic
            return Task.FromResult(0);
        });
        
        var adminCommand = new Command("admin", "Administrative commands");
        // Future implementation
        
        rootCommand.Add(testCommand);
        rootCommand.Add(adminCommand);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}
