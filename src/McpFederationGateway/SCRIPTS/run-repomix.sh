#!/bin/bash
# Run repomix-mcp and suppress stderr to avoid confusing the MCP client
# We assume "Repomix MCP server running on stdio" is printed to stderr.
# Suppressing stderr entirely might hide errors, but it's necessary if StdioClientTransport is strict or if piping issues occur.

# Ensure we use npx
# Try to silence stderr
exec npx -y repomix-mcp 2>/dev/null
