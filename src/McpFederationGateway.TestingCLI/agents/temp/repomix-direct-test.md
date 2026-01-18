# Repomix MCP Server Direct Test

## Configuration
- Command: `npx --no-update-notifier -y repomix --mcp`
- Env: NODE_NO_WARNINGS=1, NPM_CONFIG_UPDATE_NOTIFIER=false
- Timeout: 10000ms

## Results

### Server Started
YES

### Initialize Request/Response
SUCCESS

```json
{
  "result": {
    "protocolVersion": "2024-11-05",
    "capabilities": {
      "prompts": {
        "listChanged": true
      },
      "tools": {
        "listChanged": true
      }
    },
    "serverInfo": {
      "name": "repomix-mcp-server",
      "version": "1.11.0"
    },
    "instructions": "Repomix MCP Server provides AI-optimized codebase analysis tools. Use pack_codebase or pack_remote_repository to consolidate code into a single XML file, use generate_skill to create Claude Agent Skills from codebases, use attach_packed_output to work with existing packed outputs, then read_repomix_output and grep_repomix_output to analyze it. Perfect for code reviews, documentation generation, bug investigation, GitHub repository analysis, and understanding large codebases. Includes security scanning and supports compression for token efficiency."
  },
  "jsonrpc": "2.0",
  "id": 1
}
```

### tools/list Request/Response
SUCCESS

```json
{
  "result": {
    "tools": [
      {
        "name": "pack_codebase",
        "title": "Pack Local Codebase",
        "description": "Package a local code directory into a consolidated file for AI analysis. This tool analyzes the codebase structure, extracts relevant code content, and generates a comprehensive report including metrics, file tree, and formatted code content. Supports multiple output formats: XML (structured with <file> tags), Markdown (human-readable with ## headers and code blocks), JSON (machine-readable with files as key-value pairs), and Plain text (simple format with separators). Also supports Tree-sitter compression for efficient token usage.",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "directory": {
              "type": "string",
              "description": "Directory to pack (Absolute path)"
            },
            "compress": {
              "default": false,
              "description": "Enable Tree-sitter compression to extract essential code signatures and structure while removing implementation details. Reduces token usage by ~70% while preserving semantic meaning. Generally not needed since grep_repomix_output allows incremental content retrieval. Use only when you specifically need the entire codebase content for large repositories (default: false).",
              "type": "boolean"
            },
            "includePatterns": {
              "description": "Specify files to include using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"**/*.{js,ts}\", \"src/**,docs/**\"). Only matching files will be processed. Useful for focusing on specific parts of the codebase.",
              "type": "string"
            },
            "ignorePatterns": {
              "description": "Specify additional files to exclude using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"test/**,*.spec.js\", \"node_modules/**,dist/**\"). These patterns supplement .gitignore and built-in exclusions.",
              "type": "string"
            },
            "topFilesLength": {
              "default": 10,
              "description": "Number of largest files by size to display in the metrics summary for codebase analysis (default: 10)",
              "type": "integer",
              "minimum": 1,
              "maximum": 9007199254740991
            },
            "style": {
              "default": "xml",
              "description": "Output format style: xml (structured tags, default), markdown (human-readable with code blocks), json (machine-readable key-value), or plain (simple text with separators)",
              "type": "string",
              "enum": [
                "xml",
                "markdown",
                "json",
                "plain"
              ]
            }
          },
          "required": [
            "directory"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": false,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "description": {
              "type": "string",
              "description": "Human-readable description of the packing results"
            },
            "result": {
              "type": "string",
              "description": "JSON string containing detailed metrics and file information"
            },
            "directoryStructure": {
              "type": "string",
              "description": "Tree structure of the processed directory"
            },
            "outputId": {
              "type": "string",
              "description": "Unique identifier for accessing the packed content"
            },
            "outputFilePath": {
              "type": "string",
              "description": "File path to the generated output file"
            },
            "totalFiles": {
              "type": "number",
              "description": "Total number of files processed"
            },
            "totalTokens": {
              "type": "number",
              "description": "Total token count of the content"
            }
          },
          "required": [
            "description",
            "result",
            "directoryStructure",
            "outputId",
            "outputFilePath",
            "totalFiles",
            "totalTokens"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "pack_remote_repository",
        "title": "Pack Remote Repository",
        "description": "Fetch, clone, and package a GitHub repository into a consolidated file for AI analysis. This tool automatically clones the remote repository, analyzes its structure, and generates a comprehensive report. Supports multiple output formats: XML (structured with <file> tags), Markdown (human-readable with ## headers and code blocks), JSON (machine-readable with files as key-value pairs), and Plain text (simple format with separators). Also supports various GitHub URL formats and includes security checks to prevent exposure of sensitive information.",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "remote": {
              "type": "string",
              "description": "GitHub repository URL or user/repo format (e.g., \"yamadashy/repomix\", \"https://github.com/user/repo\", or \"https://github.com/user/repo/tree/branch\")"
            },
            "compress": {
              "default": false,
              "description": "Enable Tree-sitter compression to extract essential code signatures and structure while removing implementation details. Reduces token usage by ~70% while preserving semantic meaning. Generally not needed since grep_repomix_output allows incremental content retrieval. Use only when you specifically need the entire codebase content for large repositories (default: false).",
              "type": "boolean"
            },
            "includePatterns": {
              "description": "Specify files to include using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"**/*.{js,ts}\", \"src/**,docs/**\"). Only matching files will be processed. Useful for focusing on specific parts of the codebase.",
              "type": "string"
            },
            "ignorePatterns": {
              "description": "Specify additional files to exclude using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"test/**,*.spec.js\", \"node_modules/**,dist/**\"). These patterns supplement .gitignore and built-in exclusions.",
              "type": "string"
            },
            "topFilesLength": {
              "default": 10,
              "description": "Number of largest files by size to display in the metrics summary for codebase analysis (default: 10)",
              "type": "integer",
              "minimum": 1,
              "maximum": 9007199254740991
            },
            "style": {
              "default": "xml",
              "description": "Output format style: xml (structured tags, default), markdown (human-readable with code blocks), json (machine-readable key-value), or plain (simple text with separators)",
              "type": "string",
              "enum": [
                "xml",
                "markdown",
                "json",
                "plain"
              ]
            }
          },
          "required": [
            "remote"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": false,
          "openWorldHint": true
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "description": {
              "type": "string",
              "description": "Human-readable description of the packing results"
            },
            "result": {
              "type": "string",
              "description": "JSON string containing detailed metrics and file information"
            },
            "directoryStructure": {
              "type": "string",
              "description": "Tree structure of the processed repository"
            },
            "outputId": {
              "type": "string",
              "description": "Unique identifier for accessing the packed content"
            },
            "outputFilePath": {
              "type": "string",
              "description": "File path to the generated output file"
            },
            "totalFiles": {
              "type": "number",
              "description": "Total number of files processed"
            },
            "totalTokens": {
              "type": "number",
              "description": "Total token count of the content"
            }
          },
          "required": [
            "description",
            "result",
            "directoryStructure",
            "outputId",
            "outputFilePath",
            "totalFiles",
            "totalTokens"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "generate_skill",
        "title": "Generate Claude Agent Skill",
        "description": "Generate a Claude Agent Skill from a local code directory. Creates a skill package containing SKILL.md (entry point with metadata) and references/ folder with summary.md, project-structure.md, files.md, and optionally tech-stack.md.\n\nThis tool creates Project Skills in <project>/.claude/skills/<name>/, which are shared with the team via version control.\n\nOutput Structure:\n  .claude/skills/<skill-name>/\n  ├── SKILL.md                    # Entry point with usage guide\n  └── references/\n      ├── summary.md              # Purpose, format, and statistics\n      ├── project-structure.md    # Directory tree with line counts\n      ├── files.md                # All file contents\n      └── tech-stack.md           # Languages, frameworks, dependencies (if detected)\n\nExample Path:\n  /path/to/project/.claude/skills/repomix-reference-myproject/",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "directory": {
              "type": "string",
              "description": "Directory to pack (Absolute path)"
            },
            "skillName": {
              "description": "Name of the skill to generate (kebab-case, max 64 chars). Will be normalized if not in kebab-case. Used for the skill directory name and SKILL.md metadata. If omitted, auto-generates as \"repomix-reference-<folder-name>\".",
              "type": "string"
            },
            "compress": {
              "default": false,
              "description": "Enable Tree-sitter compression to extract essential code signatures and structure while removing implementation details (default: false).",
              "type": "boolean"
            },
            "includePatterns": {
              "description": "Specify files to include using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"**/*.{js,ts}\", \"src/**,docs/**\").",
              "type": "string"
            },
            "ignorePatterns": {
              "description": "Specify additional files to exclude using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"test/**,*.spec.js\").",
              "type": "string"
            }
          },
          "required": [
            "directory"
          ]
        },
        "annotations": {
          "readOnlyHint": false,
          "destructiveHint": false,
          "idempotentHint": true,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "skillPath": {
              "type": "string",
              "description": "Path to the generated skill directory"
            },
            "skillName": {
              "type": "string",
              "description": "Normalized name of the generated skill"
            },
            "totalFiles": {
              "type": "number",
              "description": "Total number of files processed"
            },
            "totalTokens": {
              "type": "number",
              "description": "Total token count of the content"
            },
            "description": {
              "type": "string",
              "description": "Human-readable description of the skill generation results"
            }
          },
          "required": [
            "skillPath",
            "skillName",
            "totalFiles",
            "totalTokens",
            "description"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "attach_packed_output",
        "title": "Attach Packed Output",
        "description": "Attach an existing Repomix packed output file for AI analysis.\nThis tool accepts either a directory containing a repomix output file or a direct path to a packed repository file.\nSupports multiple formats: XML (structured with <file> tags), Markdown (human-readable with ## headers and code blocks), JSON (machine-readable with files as key-value pairs), and Plain text (simple format with separators).\nCalling the tool again with the same file path will refresh the content if the file has been updated.\nIt will return in that case a new output ID and the updated content.",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Path to a directory containing repomix output file or direct path to a packed repository file (supports .xml, .md, .txt, .json formats)"
            },
            "topFilesLength": {
              "default": 10,
              "description": "Number of largest files by size to display in the metrics summary (default: 10)",
              "type": "integer",
              "minimum": 1,
              "maximum": 9007199254740991
            }
          },
          "required": [
            "path"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": true,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "description": {
              "type": "string",
              "description": "Human-readable description of the attached output"
            },
            "result": {
              "type": "string",
              "description": "JSON string containing detailed metrics and file information"
            },
            "directoryStructure": {
              "type": "string",
              "description": "Tree structure extracted from the packed output"
            },
            "outputId": {
              "type": "string",
              "description": "Unique identifier for accessing the packed content"
            },
            "outputFilePath": {
              "type": "string",
              "description": "File path to the attached output file"
            },
            "totalFiles": {
              "type": "number",
              "description": "Total number of files in the packed output"
            },
            "totalTokens": {
              "type": "number",
              "description": "Total token count of the content"
            }
          },
          "required": [
            "description",
            "result",
            "directoryStructure",
            "outputId",
            "outputFilePath",
            "totalFiles",
            "totalTokens"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "read_repomix_output",
        "title": "Read Repomix Output",
        "description": "Read the contents of a Repomix-generated output file. Supports partial reading with line range specification for large files. This tool is designed for environments where direct file system access is limited (e.g., web-based environments, sandboxed applications). For direct file system access, use standard file operations.",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "outputId": {
              "type": "string",
              "description": "ID of the Repomix output file to read"
            },
            "startLine": {
              "description": "Starting line number (1-based, inclusive). If not specified, reads from beginning.",
              "type": "number"
            },
            "endLine": {
              "description": "Ending line number (1-based, inclusive). If not specified, reads to end.",
              "type": "number"
            }
          },
          "required": [
            "outputId"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": true,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "content": {
              "type": "string",
              "description": "The file content or specified line range"
            },
            "totalLines": {
              "type": "number",
              "description": "Total number of lines in the file"
            },
            "linesRead": {
              "type": "number",
              "description": "Number of lines actually read"
            },
            "startLine": {
              "description": "Starting line number used",
              "type": "number"
            },
            "endLine": {
              "description": "Ending line number used",
              "type": "number"
            }
          },
          "required": [
            "content",
            "totalLines",
            "linesRead"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "grep_repomix_output",
        "title": "Grep Repomix Output",
        "description": "Search for patterns in a Repomix output file using grep-like functionality with JavaScript RegExp syntax. Returns matching lines with optional context lines around matches.",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "outputId": {
              "type": "string",
              "description": "ID of the Repomix output file to search"
            },
            "pattern": {
              "type": "string",
              "description": "Search pattern (JavaScript RegExp regular expression syntax)"
            },
            "contextLines": {
              "default": 0,
              "description": "Number of context lines to show before and after each match (default: 0). Overridden by beforeLines/afterLines if specified.",
              "type": "number"
            },
            "beforeLines": {
              "description": "Number of context lines to show before each match (like grep -B). Takes precedence over contextLines.",
              "type": "number"
            },
            "afterLines": {
              "description": "Number of context lines to show after each match (like grep -A). Takes precedence over contextLines.",
              "type": "number"
            },
            "ignoreCase": {
              "default": false,
              "description": "Perform case-insensitive matching (default: false)",
              "type": "boolean"
            }
          },
          "required": [
            "outputId",
            "pattern"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": true,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "description": {
              "type": "string",
              "description": "Human-readable description of the search results"
            },
            "matches": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "lineNumber": {
                    "type": "number",
                    "description": "Line number where the match was found"
                  },
                  "line": {
                    "type": "string",
                    "description": "The full line content"
                  },
                  "matchedText": {
                    "type": "string",
                    "description": "The actual text that matched the pattern"
                  }
                },
                "required": [
                  "lineNumber",
                  "line",
                  "matchedText"
                ],
                "additionalProperties": false
              },
              "description": "Array of search matches found"
            },
            "formattedOutput": {
              "type": "array",
              "items": {
                "type": "string"
              },
              "description": "Formatted grep-style output with context lines"
            },
            "totalMatches": {
              "type": "number",
              "description": "Total number of matches found"
            },
            "pattern": {
              "type": "string",
              "description": "The search pattern that was used"
            }
          },
          "required": [
            "description",
            "matches",
            "formattedOutput",
            "totalMatches",
            "pattern"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "file_system_read_file",
        "title": "Read File",
        "description": "Read a file from the local file system using an absolute path. Includes built-in security validation to detect and prevent access to files containing sensitive information (API keys, passwords, secrets).",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Absolute path to the file to read"
            }
          },
          "required": [
            "path"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": true,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "The file path that was read"
            },
            "content": {
              "type": "string",
              "description": "The file content"
            },
            "size": {
              "type": "number",
              "description": "File size in bytes"
            },
            "encoding": {
              "type": "string",
              "description": "Text encoding used to read the file"
            },
            "lines": {
              "type": "number",
              "description": "Number of lines in the file"
            }
          },
          "required": [
            "path",
            "content",
            "size",
            "encoding",
            "lines"
          ],
          "additionalProperties": false
        }
      },
      {
        "name": "file_system_read_directory",
        "title": "Read Directory",
        "description": "List the contents of a directory using an absolute path. Returns a formatted list showing files and subdirectories with clear [FILE]/[DIR] indicators. Useful for exploring project structure and understanding codebase organization.",
        "inputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "Absolute path to the directory to list"
            }
          },
          "required": [
            "path"
          ]
        },
        "annotations": {
          "readOnlyHint": true,
          "destructiveHint": false,
          "idempotentHint": true,
          "openWorldHint": false
        },
        "execution": {
          "taskSupport": "forbidden"
        },
        "outputSchema": {
          "$schema": "http://json-schema.org/draft-07/schema#",
          "type": "object",
          "properties": {
            "path": {
              "type": "string",
              "description": "The directory path that was listed"
            },
            "contents": {
              "type": "array",
              "items": {
                "type": "string"
              },
              "description": "Array of directory contents with [FILE]/[DIR] indicators"
            },
            "totalItems": {
              "type": "number",
              "description": "Total number of items in the directory"
            },
            "fileCount": {
              "type": "number",
              "description": "Number of files in the directory"
            },
            "directoryCount": {
              "type": "number",
              "description": "Number of subdirectories in the directory"
            }
          },
          "required": [
            "path",
            "contents",
            "totalItems",
            "fileCount",
            "directoryCount"
          ],
          "additionalProperties": false
        }
      }
    ]
  },
  "jsonrpc": "2.0",
  "id": 2
}
```

## Server Output
```
SENT: {"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0.0"}}}
STDOUT: {"result":{"protocolVersion":"2024-11-05","capabilities":{"prompts":{"listChanged":true},"tools":{"listChanged":true}},"serverInfo":{"name":"repomix-mcp-server","version":"1.11.0"},"instructions":"Repomix MCP Server provides AI-optimized codebase analysis tools. Use pack_codebase or pack_remote_repository to consolidate code into a single XML file, use generate_skill to create Claude Agent Skills from codebases, use attach_packed_output to work with existing packed outputs, then read_repomix_output and grep_repomix_output to analyze it. Perfect for code reviews, documentation generation, bug investigation, GitHub repository analysis, and understanding large codebases. Includes security scanning and supports compression for token efficiency."},"jsonrpc":"2.0","id":1}
SENT: {"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
STDOUT: {"result":{"tools":[{"name":"pack_codebase","title":"Pack Local Codebase","description":"Package a local code directory into a consolidated file for AI analysis. This tool analyzes the codebase structure, extracts relevant code content, and generates a comprehensive report including metrics, file tree, and formatted code content. Supports multiple output formats: XML (structured with <file> tags), Markdown (human-readable with ## headers and code blocks), JSON (machine-readable with files as key-value pairs), and Plain text (simple format with separators). Also supports Tree-sitter compression for efficient token usage.","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"directory":{"type":"string","description":"Directory to pack (Absolute path)"},"compress":{"default":false,"description":"Enable Tree-sitter compression to extract essential code signatures and structure while removing implementation details. Reduces token usage by ~70% while preserving semantic meaning. Generally not needed since grep_repomix_output allows incremental content retrieval. Use only when you specifically need the entire codebase content for large repositories (default: false).","type":"boolean"},"includePatterns":{"description":"Specify files to include using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"**/*.{js,ts}\", \"src/**,docs/**\"). Only matching files will be processed. Useful for focusing on specific parts of the codebase.","type":"string"},"ignorePatterns":{"description":"Specify additional files to exclude using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"test/**,*.spec.js\", \"node_modules/**,dist/**\"). These patterns supplement .gitignore and built-in exclusions.","type":"string"},"topFilesLength":{"default":10,"description":"Number of largest files by size to display in the metrics summary for codebase analysis (default: 10)","type":"integer","minimum":1,"maximum":9007199254740991},"style":{"default":"xml","description":"Output format style: xml (structured tags, default), markdown (human-readable with code blocks), json (machine-readable key-value), or plain (simple text with separators)","type":"string","enum":["xml","markdown","json","plain"]}},"required":["directory"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":false,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"description":{"type":"string","description":"Human-readable description of the packing results"},"result":{"type":"string","description":"JSON string containing detailed metrics and file information"},"directoryStructure":{"type":"string","description":"Tree structure of the processed directory"},"outputId":{"type":"string","description":"Unique identifier for accessing the packed content"},"outputFilePath":{"type":"string","description":"File path to the generated output file"},"totalFiles":{"type":"number","description":"Total number of files processed"},"totalTokens":{"type":"number","description":"Total token count of the content"}},"required":["description","result","directoryStructure","outputId","outputFilePath","totalFiles","totalTokens"],"additionalProperties":false}},{"name":"pack_remote_repository","title":"Pack Remote Repository","description":"Fetch, clone, and package a GitHub repository into a consolidated file for AI analysis. This tool automatically clones the remote repository, analyzes its structure, and generates a comprehensive report. Supports multiple output formats: XML (structured with <file> tags), Markdown (human-readable with ## headers and code blocks), JSON (machine-readable with files as key-value pairs), and Plain text (simple format with separators). Also supports various GitHub URL formats and includes security checks to prevent exposure of sensitive information.","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"remote":{"type":"string","description":"GitHub repository URL or user/repo format (e.g., \"yamadashy/repomix\", \"https://github.com/user/repo\", or \"https://github.com/user/repo/tree/branch\")"},"compress":{"default":false,"description":"Enable Tree-sitter compression to extract essential code signatures and structure while removing implementation details. Reduces token usage by ~70% while preserving semantic meaning. Generally not needed since grep_repomix_output allows incremental content retrieval. Use only when you specifically need the entire codebase content for large repositories (default: false).","type":"boolean"},"includePatterns":{"description":"Specify files to include using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"**/*.{js,ts}\", \"src/**,docs/**\"). Only matching files will be processed. Useful for focusing on specific parts of the codebase.","type":"string"},"ignorePatterns":{"description":"Specify additional files to exclude using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"test/**,*.spec.js\", \"node_modules/**,dist/**\"). These patterns supplement .gitignore and built-in exclusions.","type":"string"},"topFilesLength":{"default":10,"description":"Number of largest files by size to display in the metrics summary for codebase analysis (default: 10)","type":"integer","minimum":1,"maximum":9007199254740991},"style":{"default":"xml","description":"Output format style: xml (structured tags, default), markdown (human-readable with code blocks), json (machine-readable key-value), or plain (simple text with separators)","type":"string","enum":["xml","markdown","json","plain"]}},"required":["remote"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":false,"openWorldHint":true},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"description":{"type":"string","description":"Human-readable description of the packing results"},"result":{"type":"string","description":"JSON string containing detailed metrics and file information"},"directoryStructure":{"type":"string","description":"Tree structure of the processed repository"},"outputId":{"type":"string","description":"Unique identifier for accessing the packed content"},"outputFilePath":{"type":"string","description":"File path to the generated output file"},"totalFiles":{"type":"number","description":"Total number of files processed"},"totalTokens":{"type":"number","description":"Total token count of the content"}},"required":["description","result","directoryStructure","outputId","outputFilePath","totalFiles","totalTokens"],"additionalProperties":false}},{"name":"generate_skill","title":"Generate Claude Agent Skill","description":"Generate a Claude Agent Skill from a local code directory. Creates a skill package containing SKILL.md (entry point with metadata) and references/ folder with summary.md, project-structure.md, files.md, and optionally tech-stack.md.\n\nThis tool creates Project Skills in <project>/.claude/skills/<name>/, which are shared with the team via version control.\n\nOutput Structure:\n  .claude/skills/<skill-name>/\n  ├── SKILL.md                    # Entry point with usage guide\n  └── references/\n      ├── summary.md              # Purpose, format, and statistics\n      ├── project-structure.md    # Directory tree with line counts\n      ├── files.md                # All file contents\n      └── tech-stack.md           # Languages, frameworks, dependencies (if detected)\n\nExample Path:\n  /path/to/project/.claude/skills/repomix-reference-myproject/","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"directory":{"type":"string","description":"Directory to pack (Absolute path)"},"skillName":{"description":"Name of the skill to generate (kebab-case, max 64 chars). Will be normalized if not in kebab-case. Used for the skill directory name and SKILL.md metadata. If omitted, auto-generates as \"repomix-reference-<folder-name>\".","type":"string"},"compress":{"default":false,"description":"Enable Tree-sitter compression to extract essential code signatures and structure while removing implementation details (default: false).","type":"boolean"},"includePatterns":{"description":"Specify files to include using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"**/*.{js,ts}\", \"src/**,docs/**\").","type":"string"},"ignorePatterns":{"description":"Specify additional files to exclude using fast-glob patterns. Multiple patterns can be comma-separated (e.g., \"test/**,*.spec.js\").","type":"string"}},"required":["directory"]},"annotations":{"readOnlyHint":false,"destructiveHint":false,"idempotentHint":true,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"skillPath":{"type":"string","description":"Path to the generated skill directory"},"skillName":{"type":"string","description":"Normalized name of the generated skill"},"totalFiles":{"type":"number","description":"Total number of files processed"},"totalTokens":{"type":"number","description":"Total token count of the content"},"description":{"type":"string","description":"Human-readable description of the skill generation results"}},"required":["skillPath","skillName","totalFiles","totalTokens","description"],"additionalProperties":false}},{"name":"attach_packed_output","title":"Attach Packed Output","description":"Attach an existing Repomix packed output file for AI analysis.\nThis tool accepts either a directory containing a repomix output file or a direct path to a packed repository file.\nSupports multiple formats: XML (structured with <file> tags), Markdown (human-readable with ## headers and code blocks), JSON (machine-readable with files as key-value pairs), and Plain text (simple format with separators).\nCalling the tool again with the same file path will refresh the content if the file has been updated.\nIt will return in that case a new output ID and the updated content.","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"path":{"type":"string","description":"Path to a directory containing repomix output file or direct path to a packed repository file (supports .xml, .md, .txt, .json formats)"},"topFilesLength":{"default":10,"description":"Number of largest files by size to display in the metrics summary (default: 10)","type":"integer","minimum":1,"maximum":9007199254740991}},"required":["path"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":true,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"description":{"type":"string","description":"Human-readable description of the attached output"},"result":{"type":"string","description":"JSON string containing detailed metrics and file information"},"directoryStructure":{"type":"string","description":"Tree structure extracted from the packed output"},"outputId":{"type":"string","description":"Unique identifier for accessing the packed content"},"outputFilePath":{"type":"string","description":"File path to the attached output file"},"totalFiles":{"type":"number","description":"Total number of files in the packed output"},"totalTokens":{"type":"number","description":"Total token count of the content"}},"required":["description","result","directoryStructure","outputId","outputFilePath","totalFiles","totalTokens"],"additionalProperties":false}},{"name":"read_repomix_output","title":"Read Repomix Output","description":"Read the contents of a Repomix-generated output file. Supports partial reading with line range specification for large files. This tool is designed for environments where direct file system access is limited (e.g., web-based environments, sandboxed applications). For direct file system access, use standard file operations.","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"outputId":{"type":"string","description":"ID of the Repomix output file to read"},"startLine":{"description":"Starting line number (1-based, inclusive). If not specified, reads from beginning.","type":"number"},"endLine":{"description":"Ending line number (1-based, inclusive). If not specified, reads to end.","type":"number"}},"required":["outputId"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":true,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"content":{"type":"string","description":"The file content or specified line range"},"totalLines":{"type":"number","description":"Total number of lines in the file"},"linesRead":{"type":"number","description":"Number of lines actually read"},"startLine":{"description":"Starting line number used","type":"number"},"endLine":{"description":"Ending line number used","type":"number"}},"required":["content","totalLines","linesRead"],"additionalProperties":false}},{"name":"grep_repomix_output","title":"Grep Repomix Output","description":"Search for patterns in a Repomix output file using grep-like functionality with JavaScript RegExp syntax. Returns matching lines with optional context lines around matches.","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"outputId":{"type":"string","description":"ID of the Repomix output file to search"},"pattern":{"type":"string","description":"Search pattern (JavaScript RegExp regular expression syntax)"},"contextLines":{"default":0,"description":"Number of context lines to show before and after each match (default: 0). Overridden by beforeLines/afterLines if specified.","type":"number"},"beforeLines":{"description":"Number of context lines to show before each match (like grep -B). Takes precedence over contextLines.","type":"number"},"afterLines":{"description":"Number of context lines to show after each match (like grep -A). Takes precedence over contextLines.","type":"number"},"ignoreCase":{"default":false,"description":"Perform case-insensitive matching (default: false)","type":"boolean"}},"required":["outputId","pattern"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":true,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"description":{"type":"string","description":"Human-readable description of the search results"},"matches":{"type":"array","items":{"type":"object","properties":{"lineNumber":{"type":"number","description":"Line number where the match was found"},"line":{"type":"string","description":"The full line content"},"matchedText":{"type":"string","description":"The actual text that matched the pattern"}},"required":["lineNumber","line","matchedText"],"additionalProperties":false},"description":"Array of search matches found"},"formattedOutput":{"type":"array","items":{"type":"string"},"description":"Formatted grep-style output with context lines"},"totalMatches":{"type":"number","description":"Total number of matches found"},"pattern":{"type":"string","description":"The search pattern that was used"}},"required":["description","matches","formattedOutput","totalMatches","pattern"],"additionalProperties":false}},{"name":"file_system_read_file","title":"Read File","description":"Read a file from the local file system using an absolute path. Includes built-in security validation to detect and prevent access to files containing sensitive information (API keys, passwords, secrets).","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"path":{"type":"string","description":"Absolute path to the file to read"}},"required":["path"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":true,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"path":{"type":"string","description":"The file path that was read"},"content":{"type":"string","description":"The file content"},"size":{"type":"number","description":"File size in bytes"},"encoding":{"type":"string","description":"Text encoding used to read the file"},"lines":{"type":"number","description":"Number of lines in the file"}},"required":["path","content","size","encoding","lines"],"additionalProperties":false}},{"name":"file_system_read_directory","title":"Read Directory","description":"List the contents of a directory using an absolute path. Returns a formatted list showing files and subdirectories with clear [FILE]/[DIR] indicators. Useful for exploring project structure and understanding codebase organization.","inputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"path":{"type":"string","description":"Absolute path to the directory to list"}},"required":["path"]},"annotations":{"readOnlyHint":true,"destructiveHint":false,"idempotentHint":true,"openWorldHint":false},"execution":{"taskSupport":"forbidden"},"outputSchema":{"$schema":"http://json-schema.org/draft-07/schema#","type":"object","properties":{"path":{"type":"string","description":"The directory path that was listed"},"contents":{"type":"array","items":{"type":"string"},"description":"Array of directory contents with [FILE]/[DIR] indicators"},"totalItems":{"type":"number","description":"Total number of items in the directory"},"fileCount":{"type":"number","description":"Number of files in the directory"},"directoryCount":{"type":"number","description":"Number of subdirectories in the directory"}},"required":["path","contents","totalItems","fileCount","directoryCount"],"additionalProperties":false}}]},"jsonrpc":"2.0","id":2}
```

## Conclusion
**SERVER RESPONSIVE**: repomix MCP server started successfully and responded to both initialize and tools/list requests.
