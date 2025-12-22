using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateSlimBuilder(args);

// Logging to stderr
builder.Logging.ClearProviders();
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add simple logger middleware
builder.Services.AddHttpLogging(o => {});

builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "DummyServer", Version = "1.0.0" };
})
.WithListToolsHandler((context, ct) =>
{
    var tools = new List<Tool>
    {
        new Tool
        {
            Name = "echo",
            Description = "Echoes back the input",
            InputSchema = JsonSerializer.SerializeToDocument(new
            {
                type = "object",
                properties = new
                {
                    message = new { type = "string", description = "Message to echo" }
                },
                required = new[] { "message" }
            }).RootElement
        }
    };
    return new ValueTask<ListToolsResult>(new ListToolsResult { Tools = tools });
})
.WithCallToolHandler(async (context, ct) =>
{
    var request = context.Params;
    if (request == null) throw new Exception("Request params cannot be null");

    if (request.Name == "echo")
    {
        var msg = "no message";
        if (request.Arguments != null && request.Arguments.TryGetValue("message", out var msgObj))
        {
            msg = msgObj.ToString();
        }

        return new CallToolResult
        {
            Content = new List<ContentBlock>
            {
                new TextContentBlock { Text = $"Echo: {msg}" }
            }
        };
    }
    throw new Exception($"Tool {request.Name} not found");
})
.WithStdioServerTransport()
.WithHttpTransport();

var app = builder.Build();

app.UseHttpLogging();
app.MapMcp();

await app.RunAsync();
