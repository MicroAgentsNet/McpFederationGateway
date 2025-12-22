# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0-preview] - 2025-12-22

### Added

- Initial preview release of MCP Federation Gateway
- **Core Features**:
  - Federation and aggregation of multiple downstream MCP servers
  - Intelligent routing of tool calls and resource requests
  - Dual-transport support (Stdio and HTTP/SSE)
  - Per-server mode configuration (Direct/Federated)
  - Native AOT compilation for ultra-fast startup
  - Hierarchical configuration (user + workspace)
- **Meta-Tools**:
  - `how_to_use(mcp_server_name, topic)`: Documentation and usage guides
  - `call(mcp_server_name, tool_name, args)`: Dynamic tool invocation
- **Service Architecture**:
  - `AggregationService`: Tool aggregation from multiple servers
  - `RouterService`: Request routing and dispatching
  - `McpClientFactory`: MCP client lifecycle management
  - `ConfigurationService`: Hierarchical config merging
- **Chat Abstraction**:
  - Independent chat API (`IMicroAgentChatClient`)
  - `SamplingMicroAgentChatClient` for LLM interactions
  - Vendor extensions support
- **CLI**:
  - `McpFederationGateway.CLI` using System.CommandLine
  - Testing and admin commands
- **Testing**:
  - Integration tests with DummyServer
  - HTTP/SSE transport verification
  - All tests passing
- **Documentation**:
  - Comprehensive README with examples
  - MIT License with proper attributions (NOTICE file)
  - NuGet package metadata

### Known Limitations

- Preview release - API may change
- Limited error recovery for downstream server failures
- No UI for configuration management (planned for future)
- No policy/authorization features yet (planned for future)

### Dependencies

- .NET 10.0
- ModelContextProtocol SDK 0.5.0-preview.1
- ModelContextProtocol.AspNetCore 0.5.0-preview.1

### Installation

```bash
# Using dnx
dnx MicroAgents.McpFederationGateway@0.1.0-preview --transport stdio

# Using dotnet tool
dotnet tool install -g MicroAgents.McpFederationGateway --version 0.1.0-preview
```

### Author

Created by **Igor Solomatov**. The MicroAgentsNet GitHub organization is used for easier management of multiple projects.

---

[0.1.0-preview]: https://github.com/MicroAgentsNet/McpFederationGateway/releases/tag/v0.1.0-preview
