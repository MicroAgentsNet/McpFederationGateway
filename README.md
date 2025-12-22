# 🌐 MCP Federation Gateway

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://img.shields.io/badge/Build-Success-brightgreen.svg)]()
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![AOT](https://img.shields.io/badge/NativeAOT-Supported-orange.svg)]()

> The ultimate federated entry point for the Model Context Protocol (MCP) ecosystem.

`McpFederationGateway` is a high-performance, **Native AOT-compiled** .NET 10 service that acts as a reverse proxy and aggregator for multiple upstream MCP servers. It provides a unified interface for agents, reduces context consumption through smart aggregation, and bridges the gap between local (Stdio) and remote (SSE/HTTP) environments.

---

## ✨ Key Features

- **🔄 Federation & Aggregation**: Combines multiple downstream MCP servers into a single, unified tool catalog.
- **🛣️ Intelligent Routing**: Automatically dispatches tool calls and resource requests to the correct downstream server based on namespacing or meta-tool parameters.
- **🔌 Dual-Transport Bridge**:
  - Host it locally via **Stdio** for integration with standard MCP clients (like Claude Desktop).
  - Expose it remotely via **SSE/HTTP** for enterprise or cloud-based agentic workflows.
- **🧠 Federated Mode (Configurable per-server)**:
  - When enabled for a server, hides its specific tools from the root `tools/list` to reduce LLM context consumption. Access is provided via the `how_to_use` and `call` meta-tools.
- **🛠️ Direct Mode (Default, configurable per-server)**:
  - Exposes all tools from downstream servers as-is, with prefixes (e.g., `server_toolname`) to prevent collisions.
- **🚀 Native AOT Performance**: Compiled to native code for ultra-fast startup and minimal footprint.
- **⚙️ Hierarchical Configuration**: Merges global user defaults with workspace-specific configurations.

---

## 🛠️ Meta-Tools

In **Federated Mode**, the gateway exposes two primary tools to interact with the entire ecosystem:

| Tool         | Description                                                                                                 |
| :----------- | :---------------------------------------------------------------------------------------------------------- |
| `how_to_use` | Provides documentation, summaries, and usage guides for a specific federated MCP server using the host LLM. |
| `call`       | Dynamically invokes any tool on any federated server by specifying `server_name` and `tool_name`.           |

---

## 🚀 Getting Started

### Prerequisites

- **.NET 10 SDK**
- Downstream MCP servers (Stdio-based `npx`, `python`, etc., or SSE-based URLs)

### Installation

```bash
# Clone the repository
git clone https://github.com/MicroAgentsNet/McpFederationGateway.git
cd McpFederationGateway

# Build the project
dotnet build
```

### Configuration

The gateway looks for configuration in `~/.microagents/config.json` and local workspace roots.

**Example `config.json`**:

```json
{
  "servers": [
    {
      "name": "weather",
      "transport": "stdio",
      "mode": "direct",
      "command": "npx",
      "arguments": ["-y", "@modelcontextprotocol/server-weather"]
    },
    {
      "name": "complex-agent",
      "transport": "http",
      "mode": "federated",
      "url": "https://mcp.example.com/sse"
    }
  ]
}
```

---

## 📐 Architecture

Built on the official [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk), the gateway uses a decoupled DI-based architecture:

- **AggregationService**: Discovers and merges downstream capabilities.
- **RouterService**: Handles protocol-level dispatching and tool mapping.
- **McpClientFactory**: Manages the lifecycle of standard and SSE transports.
- **ConfigurationService**: Handles the hierarchical merging of user and root settings.

---

## 🤝 Community Pledge

We are committed to the long-term health and openness of the MicroAgents ecosystem.

- **MIT license stays forever.**
- **Monetization**: We may offer paid support, donations, sponsorships, or custom development. However, commercial features follow an "N-2" policy: features from two versions ago always become part of the community version.
- **Community Code**: Our open-source code will never depend on or include commercial code. If it does, those features will be promoted to the community tier.
- **No Warranties**: This software is provided "as-is" without warranty of any kind.

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.
