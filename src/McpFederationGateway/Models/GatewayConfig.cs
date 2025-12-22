namespace McpFederationGateway.Models;

public class GatewayConfig
{
    public List<McpServerConfig> Servers { get; set; } = new();
    public LlmConfig? Llm { get; set; }
}

public class LlmConfig
{
    public string? DefaultModel { get; set; }
    public double? Temperature { get; set; }
}
