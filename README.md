# McpFederationGateway

MCP federated gateway is a starting point. It is an MCP server, that federates access to all MCPs. It is a reverse proxy to all MCPs, with additional features like authentication, authorization, rate limiting, etc. Everything is in configuration. Configuration is for user and workspace levels (with user profile acting as default). The most important feature is that it can work both locally (stdio) and remotely for enterprises (https). The ultimate feature is how MCPs are proxied: directly (exposes all tools with all docs as-is) and federated (summarizes what each thing does). In the future it will allow to pass-through authentication, provide policies for tools, central MCP configuration, etc.

## Community Pledge

- **MIT license stays forever.**
- **Monetization**: support, donations, sponsorships, custom development, N-2 version commercial features become community features.
- **Community code** does not depend or include any commercial code. If it does, then this feature becomes community feature.
- **No warranties**.
