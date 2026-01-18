using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using McpFederationGateway.TestingCLI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace McpFederationGateway.TestingCLI.Commands;

public class TestPerformanceCommand : Command
{
    private readonly Option<int> _iterationsOption;
    private readonly Option<string?> _toolOption;

    public TestPerformanceCommand() : base("test-performance", "Execute performance benchmarks (latency, throughput)")
    {
        _iterationsOption = new Option<int>("--iterations")
        {
            Description = "Number of iterations to run",
            DefaultValueFactory = _ => 10
        };

        _toolOption = new Option<string?>("--tool")
        {
            Description = "Tool name to benchmark (optional, will list tools if not provided)"
        };

        Options.Add(_iterationsOption);
        Options.Add(_toolOption);

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult)
    {
        var iterations = parseResult.GetValue(_iterationsOption);
        var toolName = parseResult.GetValue(_toolOption);
        var logger = Program.ServiceProvider.GetRequiredService<ILogger<TestPerformanceCommand>>();
        var gatewayClient = Program.ServiceProvider.GetRequiredService<McpGatewayClient>();

        try
        {
            logger.LogInformation("=== Test Performance Command ===");
            logger.LogInformation("Iterations: {Iterations}", iterations);

            Console.WriteLine("\nPerformance Benchmark");
            Console.WriteLine(new string('=', 80));
            Console.WriteLine($"Iterations: {iterations}");

            string targetTool;
            Dictionary<string, object?> arguments;

            if (string.IsNullOrWhiteSpace(toolName))
            {
                // Default to list tools operation for benchmarking
                Console.WriteLine("No tool specified - benchmarking ListTools operation");
                Console.WriteLine(new string('-', 80));

                var latencies = new List<double>();
                var sw = Stopwatch.StartNew();

                for (int i = 0; i < iterations; i++)
                {
                    var iterationStart = Stopwatch.StartNew();
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                    var tools = await gatewayClient.ListToolsAsync(cts.Token);
                    iterationStart.Stop();

                    var latency = iterationStart.Elapsed.TotalMilliseconds;
                    latencies.Add(latency);

                    logger.LogDebug("Iteration {Iteration}/{Total}: {Latency}ms", i + 1, iterations, latency);
                    Console.Write($"\rProgress: {i + 1}/{iterations} iterations completed...");
                }

                sw.Stop();
                Console.WriteLine("\n");

                PrintStatistics(latencies, sw.Elapsed, logger);
                return 0;
            }
            else
            {
                // Benchmark specific tool call
                Console.WriteLine($"Tool: {toolName}");
                Console.WriteLine("Arguments: {} (empty)");
                Console.WriteLine(new string('-', 80));

                targetTool = toolName;
                arguments = new Dictionary<string, object?>();

                var latencies = new List<double>();
                var errors = 0;
                var sw = Stopwatch.StartNew();

                for (int i = 0; i < iterations; i++)
                {
                    var iterationStart = Stopwatch.StartNew();
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                        var result = await gatewayClient.CallToolAsync(targetTool, arguments, cts.Token);
                        iterationStart.Stop();

                        var latency = iterationStart.Elapsed.TotalMilliseconds;
                        latencies.Add(latency);

                        logger.LogDebug("Iteration {Iteration}/{Total}: {Latency}ms", i + 1, iterations, latency);
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        logger.LogWarning(ex, "Iteration {Iteration} failed", i + 1);
                    }

                    Console.Write($"\rProgress: {i + 1}/{iterations} iterations completed...");
                }

                sw.Stop();
                Console.WriteLine("\n");

                if (errors > 0)
                {
                    Console.WriteLine($"Errors: {errors}/{iterations} ({errors * 100.0 / iterations:F1}%)");
                    Console.WriteLine();
                }

                if (latencies.Count > 0)
                {
                    PrintStatistics(latencies, sw.Elapsed, logger);
                }
                else
                {
                    Console.WriteLine("All iterations failed - no statistics to display");
                    return 1;
                }

                return 0;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Test failed");
            Console.WriteLine($"\nError: {ex.Message}");
            return 1;
        }
    }

    private static void PrintStatistics(List<double> latencies, TimeSpan totalTime, ILogger logger)
    {
        latencies.Sort();

        var count = latencies.Count;
        var min = latencies.First();
        var max = latencies.Last();
        var avg = latencies.Average();
        var p50 = latencies[count / 2];
        var p95 = latencies[(int)(count * 0.95)];
        var p99 = latencies[(int)(count * 0.99)];
        var throughput = count / totalTime.TotalSeconds;

        Console.WriteLine("Results:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Total Time:    {totalTime.TotalMilliseconds:F2}ms");
        Console.WriteLine($"Throughput:    {throughput:F2} ops/sec");
        Console.WriteLine();
        Console.WriteLine("Latency Statistics:");
        Console.WriteLine($"  Min:         {min:F2}ms");
        Console.WriteLine($"  Max:         {max:F2}ms");
        Console.WriteLine($"  Average:     {avg:F2}ms");
        Console.WriteLine($"  P50:         {p50:F2}ms");
        Console.WriteLine($"  P95:         {p95:F2}ms");
        Console.WriteLine($"  P99:         {p99:F2}ms");
        Console.WriteLine(new string('=', 80));

        logger.LogInformation(
            "Performance test completed: {Count} iterations, Avg={Avg}ms, P50={P50}ms, P95={P95}ms, P99={P99}ms, Throughput={Throughput:F2} ops/sec",
            count, avg, p50, p95, p99, throughput);
    }
}
