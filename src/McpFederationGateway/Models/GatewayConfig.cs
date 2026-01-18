namespace McpFederationGateway.Models;

public record GatewayConfig
{
    public List<McpServerConfig> Servers { get; init; } = new();
    public LlmConfig? Llm { get; init; }
}

public record LlmConfig
{
    public string? DefaultModel { get; init; }
    public double? Temperature { get; init; }
}
