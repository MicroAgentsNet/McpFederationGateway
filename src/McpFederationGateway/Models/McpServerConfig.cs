using System.Text.Json.Serialization;

namespace McpFederationGateway.Models;

/// <summary>
///     The transport type used to communicate with an MCP server.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<McpTransportType>))]
public enum McpTransportType
{
    /// <summary>
    ///     Standard input/output protocol.
    /// </summary>
    Stdio,

    /// <summary>
    ///     HTTP with Server-Sent Events (SSE).
    /// </summary>
    Http
}

/// <summary>
///     Operational mode for how tools from the MCP server are exposed.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<McpOperationMode>))]
public enum McpOperationMode
{
    /// <summary>
    ///     Directly exposes tools from the MCP server with a prefix.
    /// </summary>
    Direct,

    /// <summary>
    ///     Hides tools from the main list, allowing access via meta-tools only.
    /// </summary>
    Federated
}

public record McpServerConfig
{
    public required string Name { get; init; }
    public McpTransportType Transport { get; init; } = McpTransportType.Stdio;
    public McpOperationMode Mode { get; init; } = McpOperationMode.Direct;
    public string? Command { get; init; }
    
    [JsonPropertyName("args")]
    public string[]? Arguments { get; init; }
    
    public string? Url { get; init; }
    
    [JsonPropertyName("env")]
    public Dictionary<string, string>? Env { get; init; }
}
