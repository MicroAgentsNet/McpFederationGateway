using System.Text.Json.Serialization;
using McpFederationGateway.Models;

namespace McpFederationGateway;

[JsonSerializable(typeof(GatewayConfig))]
[JsonSerializable(typeof(McpServerConfig))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}
