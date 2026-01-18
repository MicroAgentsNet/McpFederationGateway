using System.CommandLine;
using System.CommandLine.Parsing;
using McpFederationGateway.TestingCLI.Commands;
using McpFederationGateway.TestingCLI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace McpFederationGateway.TestingCLI;

public static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public static async Task<int> Main(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.secrets.json", optional: true)
            .AddJsonFile("testingcli-config.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        // Configure Serilog
        var loggerConfig = new LoggerConfiguration();

        // Read from appsettings.json Serilog section
        var serilogSection = configuration.GetSection("Serilog");
        if (serilogSection.Exists())
        {
            loggerConfig = loggerConfig.ReadFrom.Configuration(configuration);
        }
        else
        {
            // Fallback configuration
            loggerConfig = loggerConfig
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .WriteTo.File("logs/test-.log", rollingInterval: RollingInterval.Day);
        }

        Log.Logger = loggerConfig.CreateLogger();

        try
        {
            Log.Information("Starting MCP Federation Gateway Testing CLI...");

            // Setup DI
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
            });

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Add MCP Gateway Client
            services.AddSingleton<McpGatewayClient>();

            ServiceProvider = services.BuildServiceProvider();

            // Initialize MCP Gateway Client
            var gatewayClient = ServiceProvider.GetRequiredService<McpGatewayClient>();
            await gatewayClient.InitializeAsync(configuration);

            // Build command line
            var rootCommand = new RootCommand("MCP Federation Gateway Testing CLI - Test commands for gateway functionality via MCP protocol");

            rootCommand.Subcommands.Add(new TestToolsCommand());
            rootCommand.Subcommands.Add(new TestCallCommand());
            rootCommand.Subcommands.Add(new TestFederatedCommand());
            rootCommand.Subcommands.Add(new TestConfigCommand());
            rootCommand.Subcommands.Add(new TestSamplingCommand());
            rootCommand.Subcommands.Add(new TestPerformanceCommand());

            var result = await rootCommand.Parse(args).InvokeAsync();

            await gatewayClient.DisposeAsync();

            Log.Information("Testing CLI completed with exit code {ExitCode}", result);
            return result;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
