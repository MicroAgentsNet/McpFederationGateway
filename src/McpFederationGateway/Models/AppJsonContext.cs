using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;

namespace McpFederationGateway.Models;

[JsonSerializable(typeof(GatewayConfig))]
[JsonSerializable(typeof(List<McpServerConfig>))]
[JsonSerializable(typeof(LlmConfig))]
[JsonSerializable(typeof(JsonElement))]
public partial class AppJsonContext : JsonSerializerContext
{
}
