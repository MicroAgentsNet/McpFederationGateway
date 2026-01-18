# 🌐 MCP Federation Gateway

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/MicroAgents.McpFederationGateway.svg)](https://www.nuget.org/packages/MicroAgents.McpFederationGateway/)
[![Build Status](https://img.shields.io/badge/Build-Success-brightgreen.svg)](https://github.com/MicroAgentsNet/McpFederationGateway/actions)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![AOT](https://img.shields.io/badge/NativeAOT-Supported-orange.svg)]()

> ⚠️ **Status: Alpha Development**
> This project is currently in an **Alpha state** and is actively under development. Features are being implemented, and breaking changes may occur frequently. It is not yet recommended for production use.

> The ultimate federated entry point for the Model Context Protocol (MCP) ecosystem.

`McpFederationGateway` is a high-performance, **Native AOT-compiled** .NET 10 service that acts as a reverse proxy and aggregator for multiple upstream MCP servers. It provides a unified interface for agents, dramatically reduces context consumption (**up to 40% token savings**), and delivers enterprise-grade security and policy enforcement—whether you're running locally on your laptop or deploying centrally for your entire organization.

---

## 🚦 Project Status

- **Current State**: Alpha (Active Development)
- **Version**: `0.1.0-preview`
- **Goal**: Provide a production-ready federated gateway for the MCP ecosystem.
- **Completed**: Core routing, tool aggregation, multi-transport support (Stdio/SSE), and AOT compatibility.
- **In Progress**: Policy engine, authentication, and advanced documentation generation.

---

## ✨ Why MCP Federation Gateway?

### 💰 Massive Cost Savings (40% Token Reduction)

Every MCP server exposes full tool documentation to the LLM context. With 10+ servers, you can easily consume 50K+ tokens just listing available tools. **The gateway reduces this by up to 40%** through intelligent aggregation and federated mode—saving money for individual developers and enterprises alike.

### 🏢 Works for Everyone: Local Development to Enterprise Scale

**For Individual Developers (Stdio)**:

- Reduce your AI token costs by 40%
- Organize multiple MCPs in one place
- Simple configuration, zero network overhead
- Works seamlessly with Claude Desktop and other MCP clients

**For Enterprise Teams (HTTP/SSE)**:

- Deploy as a centralized gateway for all AI agents in your organization
- **Authentication & Authorization**: Control who can access which MCPs and tools
- **Policy Enforcement**: Validate tool calls before execution, prevent dangerous operations
- **Audit & Compliance**: Log all MCP interactions for security and compliance
- **Cost Optimization at Scale**: Centralized context management multiplies savings across all agents

### 🔒 Security & Safety (Local + Enterprise)

Running MCP servers directly creates risks:

- **Arbitrary Code Execution**: Developers running untrusted MCPs without approval, executing code in non-sandboxed environments
- **Dangerous Operations**: Database MCPs can delete data, cloud MCPs can delete resources, filesystem MCPs can modify critical files—all without oversight
- **No Access Control**: Standard MCP has no built-in auth, making it unsuitable for multi-user scenarios

**The gateway addresses these by**:

- Providing a single point of control for all MCP access
- (Future) Policy engine to validate and restrict dangerous operations
- (Future) Sandboxing support for untrusted MCPs

---

## ✨ Key Features

### 🔄 Federation & Aggregation

Combines multiple downstream MCP servers into a single, unified interface. Configure each server independently with its own mode, transport, and settings.

### 🛣️ Intelligent Routing

Automatically dispatches tool calls and resource requests to the correct downstream server based on:

- Tool name prefixes (Direct Mode)
- Meta-tool parameters (Federated Mode)
- Server configuration

### 🔌 Dual-Transport Architecture

The gateway supports both transports simultaneously or individually:

**Stdio (Local)**:

- Integration with desktop AI clients (Claude Desktop, etc.)
- Zero network overhead, perfect for individual development
- Get all the cost savings and safety benefits on your laptop

**HTTP/SSE (Remote)**:

- Deploy centrally for team/organization-wide access
- Multiple agents connecting to single gateway instance
- Enables enterprise features: authentication, audit logging, policy enforcement
- Same cost savings, multiplied across all users

### 🧠 Two Operating Modes (Per-Server Configurable)

#### **Direct Mode** (Default)

- **What it does**: Exposes all tools from the downstream server directly in the gateway's tool list
- **Tool naming**: Adds server name prefix to prevent collisions (e.g., `weather_get_forecast`, `database_query`)
- **Context impact**: Full tool documentation included in LLM context
- **Best for**: Small number of servers, frequently-used tools, when you want LLM to see all available tools upfront
- **Example**: Weather service, calculator, simple utilities

#### **Federated Mode**

- **What it does**: Hides specific tools from the main tool list, provides access via meta-tools
- **Context impact**: **Massive reduction** (up to 40% token savings) - only meta-tool docs in context, not individual tools
- **Access method**: Use `how_to_use(server_name, topic)` to get docs, `call(server_name, tool_name, args)` to invoke
- **Best for**: Large MCP servers with many tools, rarely-used tools, complex documentation
- **Example**: Database MCP with 50+ tools, cloud provider MCPs, complex agent frameworks

#### **How the Tool List is Formed**

When an AI agent queries `tools/list`, the gateway returns:

1. **All Direct Mode tools** with prefixes:

   - `weather_get_forecast` (from weather server in Direct Mode)
   - `calc_add` (from calculator server in Direct Mode)

2. **Meta-tools** (always included):

   - `how_to_use(mcp_server_name, topic)` - Get documentation for any Federated Mode server
   - `call(mcp_server_name, tool_name, args)` - Invoke any tool on any Federated Mode server

3. **Federated Mode tools are NOT listed** - they're accessed dynamically via meta-tools, keeping context lean

**Example Configuration**:

```json
{
  "servers": [
    {
      "name": "weather",
      "mode": "direct",
      "transport": "stdio",
      "command": "npx",
      "arguments": ["-y", "@modelcontextprotocol/server-weather"]
    },
    {
      "name": "database",
      "mode": "federated",
      "transport": "http",
      "url": "https://db-mcp.internal.company.com/sse"
    }
  ]
}
```

**Result**: Agent sees `weather_get_forecast`, `how_to_use`, `call` in tool list. Database tools (potentially 50+) are hidden, accessed only when needed via `call("database", "query", {...})`.

### 🚀 Native AOT Performance

Compiled to native code for ultra-fast startup and minimal footprint. Perfect for serverless and containerized deployments.

### ⚙️ Hierarchical Configuration

Merges global user defaults with workspace-specific configurations. Enterprise admins can set organization-wide policies while allowing workspace customization.

### 🔮 Future Enterprise Features (Roadmap)

- **Policy Engine**: Define rules like "database delete operations require approval" or "cloud resource modifications only in dev environment"
- **Authentication**: OAuth2, API keys, JWT tokens for secure access
- **Rate Limiting**: Prevent abuse and control costs
- **Sandboxing**: Run untrusted MCPs in isolated containers
- **Central MCP Registry**: Discover and install approved MCPs from organization catalog
- **Usage Analytics**: Track token consumption, tool usage, and costs per user/team

---

## 🛠️ Meta-Tools

In **Federated Mode**, the gateway exposes two primary tools to interact with the entire ecosystem:

| Tool         | Description                                                                                                 |
| :----------- | :---------------------------------------------------------------------------------------------------------- |
| `how_to_use` | Provides documentation, summaries, and usage guides for a specific federated MCP server using the host LLM. |
| `call`       | Dynamically invokes any tool on any federated server by specifying `server_name` and `tool_name`.           |

---

## 🧪 Testing CLI

The `McpFederationGatewayTestingCLI` is a comprehensive testing harness designed to validate and benchmark the MCP Federation Gateway functionality. It provides automated testing capabilities for tool discovery, invocation, configuration validation, and performance analysis.

### Key Features

- **6 Test Commands**: Comprehensive coverage of gateway functionality
- **Verbose Logging**: Dual output to console and file for detailed diagnostics
- **Modern CLI**: Built with System.CommandLine 2.0.1 for robust argument parsing
- **Non-Interactive**: All tests accept input via command-line arguments only

### Available Test Commands

| Command              | Description                                                                 |
| :------------------- | :-------------------------------------------------------------------------- |
| `test-tools`         | List all available tools from configured MCPs via the gateway               |
| `test-call`          | Direct tool invocation with specified arguments                             |
| `test-federated`     | Test federated mode meta-tools (`how_to_use`, `call`)                       |
| `test-config`        | Configuration validation and hierarchical merging (user + workspace)        |
| `test-sampling`      | MCP sampling functionality tests                                            |
| `test-performance`   | Performance benchmarks including latency (p50/p95/p99) and throughput tests |

### Configuration Files

The Testing CLI uses the following configuration files:

- `appsettings.json`: Logging configuration and test settings
- `appsettings.secrets.json`: API keys and sensitive configuration (git-ignored)
- `testingcli-config.json`: MCP server definitions and gateway configuration

### Usage Example

```bash
# List all available tools from configured MCPs
dotnet run --project src/McpFederationGateway.TestingCLI -- test-tools

# Test direct tool invocation with Context7
dotnet run --project src/McpFederationGateway.TestingCLI -- test-call \
  --tool context7_query_docs \
  --args '{"libraryId":"/dotnet/command-line-api","query":"latest version"}'

# Test federated mode with Playwright MCP
dotnet run --project src/McpFederationGateway.TestingCLI -- test-federated \
  --server playwright \
  --tool screenshot \
  --args '{"url":"https://example.com"}'

# Run performance benchmarks
dotnet run --project src/McpFederationGateway.TestingCLI -- test-performance \
  --iterations 10
```

All test executions generate detailed logs in the `logs/` directory with timestamps, including raw requests and responses for debugging purposes.

---

### 🚀 Running with DNX

The `McpFederationGateway` is designed to be executed using **dnx**, a lightweight runner for .NET-based MCP servers.

```bash
# To run the latest preview version from NuGet
dnx MicroAgents.McpFederationGateway --transport stdio

# To run a specific version
dnx MicroAgents.McpFederationGateway@0.1.0-preview --transport stdio

# Alternative: Install as a global .NET tool
dotnet tool install -g MicroAgents.McpFederationGateway --version 0.1.0-preview
McpFederationGateway --transport stdio
```

For development and local execution from source:

```bash
# Clone the repository
git clone https://github.com/MicroAgentsNet/McpFederationGateway.git
cd McpFederationGateway

# Run using dotnet run
dotnet run --project src/McpFederationGateway -- --transport stdio
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
