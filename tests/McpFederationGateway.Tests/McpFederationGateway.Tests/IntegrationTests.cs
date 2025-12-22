using NUnit.Framework;
using McpFederationGateway.Services;
using McpFederationGateway.Models;
using McpFederationGateway.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using System.Text.Json;

namespace McpFederationGateway.Tests;

public class DummyChatClient : IMicroAgentChatClient
{
    public Task<ChatResponse> GetResponseAsync(MicroAgentChatRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatResponse { Text = "Dummy Response" });
    }
}

public class IntegrationTests
{
    private static ServiceProvider _serviceProvider;
    private static string _configPath;
    private static System.Diagnostics.Process _serverProcess;

    [OneTimeSetUp]
    public void Setup()
    {
        var pwd = Directory.GetCurrentDirectory();
        TestContext.Progress.WriteLine($"PWD: {pwd}");
        
        _configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../tests/test_config.json"));
        
        TestContext.Progress.WriteLine($"Config Path: {_configPath}, Exists: {File.Exists(_configPath)}");
        if (File.Exists(_configPath))
        {
             TestContext.Progress.WriteLine($"Config Content: {File.ReadAllText(_configPath)}");
        }
        
        Environment.SetEnvironmentVariable("MICROAGENTS_CONFIG", _configPath);

        // Start Dummy Server
        // Ensure path includes build output directory
        // In CI, configurations might be Release. Local might be Debug.
        // We assume we are running from bin/Debug/net10.0 or bin/Release/net10.0 of the test project.
        var buildConfig = "Debug";
        #if RELEASE
        buildConfig = "Release";
        #endif
        
        // Relative path to DummyServer dll
        var serverDll = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, $"../../../../DummyServer/bin/{buildConfig}/net10.0/DummyServer.dll"));
        
        if (!File.Exists(serverDll))
        {
             Assert.Fail($"DummyServer.dll not found at {serverDll}. Please build tests/DummyServer project.");
        }

        var startInfo = new System.Diagnostics.ProcessStartInfo("dotnet", $"\"{serverDll}\" --urls=http://127.0.0.1:8090")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(serverDll),
            RedirectStandardOutput = false, // Keep false to see logs
            RedirectStandardError = false
        };
        _serverProcess = System.Diagnostics.Process.Start(startInfo);
        
        if (_serverProcess == null || _serverProcess.HasExited)
        {
             Assert.Fail("Failed to start Dummy Server process.");
        }
        
        // Wait for server to be ready
        var httpClient = new System.Net.Http.HttpClient();
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
        var startTime = DateTime.UtcNow;
        var ready = false;
        while ((DateTime.UtcNow - startTime).TotalSeconds < 15)
        {
            if (_serverProcess.HasExited)
            {
                 Assert.Fail($"Dummy Server process exited unexpectedly with code {_serverProcess.ExitCode}");
            }

            try
            {
                using var response = httpClient.GetAsync("http://127.0.0.1:8090/").Result;
                // If we get a response, the server is up and listening.
                ready = true;
                break;
            }
            catch
            {
                // Ignore connectivity errors while starting
                Thread.Sleep(500);
            }
        }
        
        if (!ready)
        {
             Assert.Fail("Dummy Server failed to start within 15 seconds or responded with non-success.");
        }

        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IMcpClientFactory, McpClientFactory>();
        services.AddSingleton<IAggregationService, AggregationService>();
        // Use dummy chat client for integration tests where chat isn't the focus
        services.AddScoped<IMicroAgentChatClient, DummyChatClient>();
        services.AddScoped<IRouterService, RouterService>();
        
        // Logging
        services.AddLogging(builder => builder.AddConsole());

        _serviceProvider = services.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
        
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try
            {
                _serverProcess.Kill();
                _serverProcess.WaitForExit(1000);
            }
            catch {}
            _serverProcess.Dispose();
        }
    }

    [Test]
    public async Task Test_Gateway_Aggregation_DirectMode()
    {
        var aggregator = _serviceProvider.GetRequiredService<IAggregationService>();
        var tools = await aggregator.GetToolsAsync();

        Assert.That(tools, Is.Not.Empty, "Tools list should not be empty");
        
        var echoTool = tools.FirstOrDefault(t => t.Name == "directory_echo"); // Prefix logic assumes defaults?
        // Wait, AggregationService prefixes tools with server name. 
        // Test config server name is "dummy". 
        // So tool should be "dummy_echo".
        
        // Check actual logic: Implementation defaults: Config name is "dummy".
        // AggregationService logic: $"{serverName}_{tool.Name}"
        
        // Let's verify if echoTool is null to debug
        var toolNames = string.Join(", ", tools.Select(t => t.Name));
        TestContext.WriteLine($"Found tools: {toolNames}");

        echoTool = tools.FirstOrDefault(t => t.Name == "dummy_echo");
        Assert.That(echoTool, Is.Not.Null, $"dummy_echo tool should exist. Found: {toolNames}");
        Assert.That(echoTool.Description, Does.Contain("Echoes back the input"));
    }

    [Test]
    public async Task Test_Gateway_Call_DirectTool()
    {
        using var scope = _serviceProvider.CreateScope();
        var router = scope.ServiceProvider.GetRequiredService<IRouterService>();
        
        var args = new Dictionary<string, object?> { { "message", "Hello Integration" } };
        
        var result = await router.CallToolAsync("dummy_echo", args, CancellationToken.None);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Content, Is.Not.Empty);
        
        var textBlock = result.Content[0] as TextContentBlock;
        Assert.That(textBlock, Is.Not.Null);
        Assert.That(textBlock.Text, Is.EqualTo("Echo: Hello Integration"));
    }
}
