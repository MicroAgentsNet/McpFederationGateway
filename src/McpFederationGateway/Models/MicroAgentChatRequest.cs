using System.Text.Json.Serialization;

namespace McpFederationGateway.Models;

/// <summary>
///     Chat outbound features supported only by a single/few providers with no shared equivalent.
/// </summary>
public record ChatRequestVendorExtensions
{
    public AnthropicExtensions? Anthropic { get; init; }
    public GoogleExtensions? Google { get; init; }
}

public record AnthropicExtensions
{
    public string? Thinking { get; init; }
}

public record GoogleExtensions
{
    public bool? UseSearch { get; init; }
}

public record MicroAgentChatRequest
{
    public IList<ChatMessage> Messages { get; init; } = new List<ChatMessage>();
    public ChatOptions? Options { get; init; }
    public ChatRequestVendorExtensions? VendorExtensions { get; init; }
}
