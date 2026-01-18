using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;

namespace McpFederationGateway.TestingCLI.Services;

public class McpGatewayClient : IAsyncDisposable
{
    private readonly ILogger<McpGatewayClient> _logger;
    private McpClient? _client;
    private bool _initialized;

    public McpGatewayClient(ILogger<McpGatewayClient> logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync(IConfiguration configuration)
    {
        if (_initialized)
        {
            _logger.LogWarning("MCP Gateway Client already initialized");
            return;
        }

        _logger.LogInformation("Initializing MCP Gateway Client...");

        try
        {
            var gatewayConfig = configuration.GetSection("mcpGateway");
            var transport = gatewayConfig["transport"] ?? throw new InvalidOperationException("Transport not configured");
            var command = gatewayConfig["command"] ?? throw new InvalidOperationException("Command not configured");
            var args = gatewayConfig.GetSection("args").Get<string[]>() ?? Array.Empty<string>();
            var env = gatewayConfig.GetSection("env").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            var openAiKey = configuration["ApiKeys:OpenAI"];

            _logger.LogInformation("Connecting to gateway via {Transport}: {Command} {Args}",
                transport, command, string.Join(" ", args));

            if (transport != "stdio")
            {
                throw new NotSupportedException($"Transport '{transport}' is not supported. Only 'stdio' is currently supported.");
            }

            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "McpFederationGateway.TestingCLI",
                Command = command,
                Arguments = args,
                EnvironmentVariables = env.ToDictionary(k => k.Key, v => (string?)v.Value)
            });

            var options = new McpClientOptions
            {
                ClientInfo = new Implementation { Name = "McpFederationGateway.TestingCLI", Version = "1.0.0" },
                Capabilities = new ClientCapabilities { Sampling = new() }
            };

            if (!string.IsNullOrEmpty(openAiKey))
            {
                _logger.LogInformation("Configuring Sampling support with OpenAI...");
                var chatClient = new ChatClient("gpt-5-mini", openAiKey);

                options.Handlers = new McpClientHandlers
                {
                    SamplingHandler = async (requestParams, progress, token) =>
                    {
                        _logger.LogInformation(">>> SAMPLING HANDLER INVOKED <<<");
                        if (requestParams == null) throw new ArgumentNullException(nameof(requestParams));
                        return await HandleSamplingAsync(chatClient, requestParams, token);
                    }
                };
            }

            _client = await McpClient.CreateAsync(clientTransport, options);

            _initialized = true;
            _logger.LogInformation("MCP Gateway Client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MCP Gateway Client");
            throw;
        }
    }

    private async Task<CreateMessageResult> HandleSamplingAsync(ChatClient chatClient, CreateMessageRequestParams request, CancellationToken ct)
    {
        try
        {
            if (request.ModelPreferences?.Hints?.Where(h => !string.IsNullOrWhiteSpace(h.Name)).Any() ?? false)
            {
                _logger.LogInformation("Sampling Request for model preference: {Hints}", string.Join(", ", request.ModelPreferences?.Hints?.Where(h => !string.IsNullOrWhiteSpace(h.Name)).Select(h => h.Name) ?? Array.Empty<string>()));
            }

            var messages = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(request.SystemPrompt))
            {
                _logger.LogInformation("Sampling Request system prompt: {SystemPrompt}", request.SystemPrompt);
                messages.Add(new SystemChatMessage(request.SystemPrompt));
            }

            foreach (var msg in request.Messages)
            {
                var text = string.Join("\n", msg.Content.OfType<TextContentBlock>().Select(c => c.Text));
                if (string.IsNullOrEmpty(text)) continue;

                _logger.LogInformation("Sampling Request message: {Role} {Message}", msg.Role, text);

                if (msg.Role == Role.User) messages.Add(new UserChatMessage(text));
                else if (msg.Role == Role.Assistant) messages.Add(new AssistantChatMessage(text));
            }

            _logger.LogInformation("Sending {MessageCount} messages to OpenAI (model: gpt-5-mini)...", messages.Count);

            var completionTime = Stopwatch.StartNew();
            var completion = await chatClient.CompleteChatAsync(messages, cancellationToken: ct);
            completionTime.Stop();
            _logger.LogInformation("Received response from OpenAI in {CompletionTime}");

            var responseText = completion.Value.Content[0].Text;

            return new CreateMessageResult
            {
                Role = Role.Assistant,
                Content = new ContentBlock[] { new TextContentBlock { Text = responseText } },
                Model = "gpt-5-mini",
                StopReason = "end_turn"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI sampling failed: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Tool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        // ... (existing)

        _logger.LogInformation("Requesting tools list from gateway...");

        try
        {
            var mcpTools = await _client!.ListToolsAsync(cancellationToken: cancellationToken);
            var tools = mcpTools.Select(t => t.ProtocolTool).ToList();

            _logger.LogInformation("Received {ToolCount} tools from gateway", tools.Count);

            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list tools");
            throw;
        }
    }

    public async Task<CallToolResult> CallToolAsync(string toolName, IDictionary<string, object?> arguments, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();

        _logger.LogInformation("Calling tool: {ToolName}", toolName);
        _logger.LogDebug("Arguments: {Arguments}", JsonSerializer.Serialize(arguments));

        try
        {
            var startTime = DateTime.UtcNow;

            var readOnlyArgs = arguments as IReadOnlyDictionary<string, object?> ?? new Dictionary<string, object?>(arguments);
            var result = await _client!.CallToolAsync(toolName, readOnlyArgs);

            var elapsed = DateTime.UtcNow - startTime;
            _logger.LogInformation("Tool call completed in {ElapsedMs}ms", elapsed.TotalMilliseconds);
            _logger.LogDebug("Result: {Result}", JsonSerializer.Serialize(result));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call tool: {ToolName}", toolName);
            throw;
        }
    }

    private void EnsureInitialized()
    {
        if (!_initialized || _client == null)
        {
            throw new InvalidOperationException("Client not initialized. Call InitializeAsync first.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
        {
            _logger.LogInformation("Disposing MCP Gateway Client...");
            await _client.DisposeAsync();
            _client = null;
        }

        _initialized = false;
    }
}
