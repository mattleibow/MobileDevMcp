# MobileDevMcp Integration Tests

This project contains comprehensive integration tests that validate end-to-end functionality of MobileDevMcp tools with AI clients, specifically demonstrating real-world deployment patterns.

## Test Categories

### 1. Real-World MCP Integration Tests (`RealWorldMcpIntegrationTests`)

These tests demonstrate the production deployment pattern where:

1. **MCP Server Process**: Starts the MobileDevMcp.Host as a separate process (simulating production deployment)
2. **Tool Discovery**: Discovers available tools from the running server
3. **AI Integration**: Converts discovered tools to Microsoft.Extensions.AI functions
4. **Azure OpenAI Usage**: Uses natural prompts with Azure OpenAI that trigger tool usage

**Key Tests:**
- `McpServer_CanBeStartedAsSeparateProcess`: Validates server startup
- `McpServer_ToolsCanBeDiscoveredAndUsedWithAI`: End-to-end tool discovery and AI integration
- `McpServer_MultipleToolsCanBeUsedInConversation`: Multi-tool scenarios

### 2. MCP Server Integration Tests (`McpAiIntegrationTests`)

These tests validate MCP server hosting and direct tool invocation patterns:
- Server instantiation with all tools
- Direct tool invocation via MCP interfaces
- Tool metadata consistency
- Function conversion patterns

### 3. Azure OpenAI Integration Framework (`AzureOpenAIIntegrationTests`)

Infrastructure for Azure OpenAI integration testing with proper credential handling.

## Azure OpenAI Configuration

To enable full AI integration tests, set these environment variables:

```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4"  # Optional, defaults to "gpt-4"
```

When these are not configured, tests will skip Azure OpenAI calls but still validate the integration patterns.

## Key Integration Patterns

### Production Deployment Pattern

```csharp
// 1. Start MCP server as separate process (like production)
await StartMcpServerProcessAsync();

// 2. Discover tools from running server
var availableTools = await DiscoverAvailableToolsFromServerAsync();

// 3. Convert to AI functions
var aiFunctions = CreateAIFunctionsFromDiscoveredTools(availableTools);

// 4. Use with Azure OpenAI
var chatOptions = new ChatOptions { Tools = aiFunctions };
var response = await chatClient.GetResponseAsync(messages, chatOptions);
```

### Tool Discovery Simulation

Currently simulates the MCP protocol tool discovery. In a full implementation, this would:
- Connect to the MCP server via stdin/stdout transport
- Send `list_tools` requests via MCP protocol
- Receive tool metadata and convert to AI functions
- Handle `call_tool` requests back to the server

## Running Tests

```bash
# Run all integration tests
dotnet test tests/MobileDevMcp.IntegrationTests/

# Run only real-world integration tests
dotnet test tests/MobileDevMcp.IntegrationTests/ --filter "RealWorldMcpIntegrationTests"

# Run with Azure OpenAI (requires environment variables)
AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/" \
AZURE_OPENAI_API_KEY="your-api-key" \
dotnet test tests/MobileDevMcp.IntegrationTests/
```

## Test Results

- ✅ Server process startup and management
- ✅ Tool discovery patterns  
- ✅ AI function conversion
- ✅ Integration pattern validation
- ⏭️ Azure OpenAI calls (when credentials available)

## CI/CD Considerations

- Tests automatically skip Azure OpenAI integration when credentials not available
- Server process management handles cleanup properly
- Tests validate the deployment patterns users would actually use
- Demonstrates client-server separation like production scenarios

This provides a solid foundation for validating that MobileDevMcp tools work correctly when deployed as separate services and integrated with AI assistants.