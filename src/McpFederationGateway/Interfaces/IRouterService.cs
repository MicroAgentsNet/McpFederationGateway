using ModelContextProtocol.Protocol;

namespace McpFederationGateway.Interfaces;

public interface IRouterService
{
    Task<CallToolResult> CallToolAsync(string fullName, IDictionary<string, object?> arguments, CancellationToken cancellationToken);
}
