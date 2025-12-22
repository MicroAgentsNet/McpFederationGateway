using ModelContextProtocol.Protocol;

namespace McpFederationGateway.Interfaces;

public interface IAggregationService
{
    Task<IEnumerable<Tool>> GetToolsAsync();
}
