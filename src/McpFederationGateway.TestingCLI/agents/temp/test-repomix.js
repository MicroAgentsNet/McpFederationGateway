#!/usr/bin/env node

const { spawn } = require('child_process');
const readline = require('readline');

const TIMEOUT_MS = 10000; // 10 second timeout

async function testRepomixMCP() {
    console.log('Starting repomix MCP server...');

    const childProcess = spawn('npx', ['--no-update-notifier', '-y', 'repomix', '--mcp'], {
        stdio: ['pipe', 'pipe', 'pipe'],
        env: {
            ...process.env,
            NODE_NO_WARNINGS: '1',
            NPM_CONFIG_UPDATE_NOTIFIER: 'false'
        }
    });

    let initResponse = null;
    let toolsResponse = null;
    let error = null;
    let startupOutput = [];

    // Track stderr for any warnings/errors
    childProcess.stderr.on('data', (data) => {
        const message = data.toString();
        startupOutput.push(`STDERR: ${message}`);
        console.error('STDERR:', message);
    });

    // Read line-by-line from stdout
    const rl = readline.createInterface({
        input: childProcess.stdout,
        crlfDelay: Infinity
    });

    const responses = [];
    rl.on('line', (line) => {
        console.log('RECEIVED:', line);
        startupOutput.push(`STDOUT: ${line}`);

        try {
            const msg = JSON.parse(line);
            responses.push(msg);

            if (msg.id === 1) {
                initResponse = msg;
            } else if (msg.id === 2) {
                toolsResponse = msg;
            }
        } catch (e) {
            console.error('Failed to parse:', line, e.message);
        }
    });

    // Helper to send request
    function sendRequest(request) {
        const json = JSON.stringify(request);
        console.log('SENDING:', json);
        startupOutput.push(`SENT: ${json}`);
        childProcess.stdin.write(json + '\n');
    }

    // Wait for process to be ready (short delay)
    await new Promise(resolve => setTimeout(resolve, 1000));

    try {
        // Send initialize request
        const initRequest = {
            jsonrpc: '2.0',
            id: 1,
            method: 'initialize',
            params: {
                protocolVersion: '2024-11-05',
                capabilities: {},
                clientInfo: {
                    name: 'test-client',
                    version: '1.0.0'
                }
            }
        };
        sendRequest(initRequest);

        // Wait for response
        await Promise.race([
            new Promise(resolve => {
                const checkInterval = setInterval(() => {
                    if (initResponse) {
                        clearInterval(checkInterval);
                        resolve();
                    }
                }, 100);
            }),
            new Promise((_, reject) => setTimeout(() => reject(new Error('Initialize timeout')), TIMEOUT_MS))
        ]);

        console.log('Initialize response received');

        // Send tools/list request
        const toolsRequest = {
            jsonrpc: '2.0',
            id: 2,
            method: 'tools/list',
            params: {}
        };
        sendRequest(toolsRequest);

        // Wait for tools response
        await Promise.race([
            new Promise(resolve => {
                const checkInterval = setInterval(() => {
                    if (toolsResponse) {
                        clearInterval(checkInterval);
                        resolve();
                    }
                }, 100);
            }),
            new Promise((_, reject) => setTimeout(() => reject(new Error('tools/list timeout')), TIMEOUT_MS))
        ]);

        console.log('tools/list response received');

    } catch (e) {
        error = e.message;
        console.error('ERROR:', e.message);
    } finally {
        // Cleanup
        childProcess.kill();
    }

    // Write results
    const fs = require('fs');
    const output = `# Repomix MCP Server Direct Test

## Configuration
- Command: \`npx --no-update-notifier -y repomix --mcp\`
- Env: NODE_NO_WARNINGS=1, NPM_CONFIG_UPDATE_NOTIFIER=false
- Timeout: ${TIMEOUT_MS}ms

## Results

### Server Started
${error ? 'NO - Error: ' + error : 'YES'}

### Initialize Request/Response
${initResponse ? 'SUCCESS' : 'FAILED - ' + (error || 'No response')}

${initResponse ? '```json\n' + JSON.stringify(initResponse, null, 2) + '\n```' : ''}

### tools/list Request/Response
${toolsResponse ? 'SUCCESS' : 'FAILED - ' + (error || 'No response')}

${toolsResponse ? '```json\n' + JSON.stringify(toolsResponse, null, 2) + '\n```' : ''}

## Server Output
\`\`\`
${startupOutput.join('\n')}
\`\`\`

## Conclusion
${error ?
  `**DEADLOCK/TIMEOUT DETECTED**: ${error}` :
  `**SERVER RESPONSIVE**: repomix MCP server started successfully and responded to both initialize and tools/list requests.`
}
`;

    fs.writeFileSync('/Users/igor/Sources/MicroAgentsNet/repos/McpFederationGateway/src/McpFederationGateway.TestingCLI/agents/temp/repomix-direct-test.md', output);
    console.log('\nResults written to agents/temp/repomix-direct-test.md');
}

testRepomixMCP().catch(console.error);
