using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;

namespace McpFederationGateway.Models;

[JsonSerializable(typeof(GatewayConfig))]
[JsonSerializable(typeof(List<McpServerConfig>))]
[JsonSerializable(typeof(McpServerConfig))]
[JsonSerializable(typeof(LlmConfig))]
[JsonSerializable(typeof(McpTransportType))]
[JsonSerializable(typeof(McpOperationMode))]
[JsonSerializable(typeof(ChatMessage))]
[JsonSerializable(typeof(List<ChatMessage>))]
[JsonSerializable(typeof(ChatOptions))]
[JsonSerializable(typeof(ChatResponse))]
[JsonSerializable(typeof(ChatRole))]
[JsonSerializable(typeof(MicroAgentChatRequest))]
[JsonSerializable(typeof(ChatRequestVendorExtensions))]
[JsonSerializable(typeof(AnthropicExtensions))]
[JsonSerializable(typeof(GoogleExtensions))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(JsonElement))]
public partial class AppJsonContext : JsonSerializerContext
{
}
