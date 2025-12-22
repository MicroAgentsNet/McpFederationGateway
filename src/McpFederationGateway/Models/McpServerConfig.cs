namespace McpFederationGateway.Models;

public class McpServerConfig
{
    public string Name { get; set; } = string.Empty;
    public string Transport { get; set; } = "stdio"; // stdio or http
    public string? Command { get; set; }
    public string[]? Arguments { get; set; }
    public string? Url { get; set; }
}
