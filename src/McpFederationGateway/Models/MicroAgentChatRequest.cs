using Microsoft.Extensions.AI;

namespace McpFederationGateway.Models;

/// <summary>
///     Chat outbound features supported only by a single/few providers with no shared equivalent.
/// </summary>
public class ChatRequestVendorExtensions
{
    public AnthropicExtensions? Anthropic { get; set; }
    public GoogleExtensions? Google { get; set; }
}

public class AnthropicExtensions
{
    public string? Thinking { get; set; }
}

public class GoogleExtensions
{
    public bool? UseSearch { get; set; }
}

public class MicroAgentChatRequest
{
    public IList<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ChatOptions? Options { get; set; }
    public ChatRequestVendorExtensions? VendorExtensions { get; set; }
}
