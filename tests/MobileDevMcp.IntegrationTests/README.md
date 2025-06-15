# MobileDevMcp Integration Tests

This project contains integration tests that validate the end-to-end functionality of the MobileDevMcp server with AI clients.

## Test Categories

### 1. MCP Server Integration Tests
- **McpServer_CanBeCreated**: Validates that the MCP server can be instantiated with all tools
- **DateTool_CanBeInvokedDirectly**: Tests direct invocation of the DateTool
- **AndroidDevicesTool_CanBeInvokedDirectly**: Tests direct invocation of Android device listing
- **AllRegisteredTools_HaveCorrectMetadata**: Validates tool metadata consistency

### 2. AI Function Integration Tests
- **McpTools_CanBeConvertedToAiFunctions**: Tests conversion of MCP tools to Microsoft.Extensions.AI functions
- **AiFunctions_HaveCorrectMetadata**: Validates converted function metadata

### 3. Azure OpenAI Integration Tests
- **AiClient_CanUseMcpTools_WithAzureOpenAI**: End-to-end test with Azure OpenAI (requires configuration)

## Azure OpenAI Configuration

To enable the full Azure OpenAI integration tests, set the following environment variables:

```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT_NAME="gpt-4"  # Optional, defaults to "gpt-4"
```

### Using .env file (for development)

Create a `.env` file in the test project directory:

```
AZURE_OPENAI_ENDPOINT=https://your-resource.openai.azure.com/
AZURE_OPENAI_API_KEY=your-api-key
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4
```

### Using GitHub Actions (for CI/CD)

Add these as repository secrets:
- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY`
- `AZURE_OPENAI_DEPLOYMENT_NAME`

## Running the Tests

### Running all tests (including skipped Azure OpenAI tests)
```bash
dotnet test tests/MobileDevMcp.IntegrationTests/
```

### Running only tests that don't require Azure OpenAI
```bash
dotnet test tests/MobileDevMcp.IntegrationTests/ --filter "FullyQualifiedName!~WithAzureOpenAI"
```

### Running with detailed output
```bash
dotnet test tests/MobileDevMcp.IntegrationTests/ -v normal
```

## Test Behavior

### When Azure OpenAI is NOT configured:
- Basic MCP server tests run normally
- AI function conversion tests run with mock validation
- Azure OpenAI specific tests are skipped with appropriate messages

### When Azure OpenAI IS configured:
- All tests run including end-to-end AI integration
- Tests validate that AI can successfully use MCP tools
- Function calling and tool invocation are verified

## Architecture

The integration tests demonstrate the following integration pattern:

1. **MCP Server Setup**: Host the MCP server with registered tools
2. **Tool Conversion**: Convert MCP tools to Microsoft.Extensions.AI functions using `AIFunctionFactory.Create()`
3. **AI Client Configuration**: Set up Azure OpenAI client with tool access
4. **End-to-End Testing**: Verify AI can successfully invoke tools and get results

### Example Integration Flow

```csharp
// 1. Start MCP server
var host = CreateMcpHost();

// 2. Convert MCP tool to AI function
var dateFunction = AIFunctionFactory.Create(async () =>
{
    var response = await dateTool.InvokeAsync(null, CancellationToken.None);
    return response.Content.FirstOrDefault()?.Text ?? "Unable to get date";
}, "get_current_date", "Get the current date");

// 3. Configure AI client with tools
var options = new ChatOptions { Tools = [dateFunction] };

// 4. Test AI tool usage
var response = await chatClient.CompleteAsync([
    new ChatMessage(ChatRole.User, "What is today's date?")
], options);
```

## Dependencies

- **Microsoft.Extensions.AI**: Core AI abstractions and function factory
- **Microsoft.Extensions.AI.OpenAI**: Azure OpenAI integration
- **Azure.AI.OpenAI**: Azure OpenAI client library
- **Microsoft.Extensions.Hosting.Testing**: Test hosting infrastructure

## Future Enhancements

- [ ] Add more complex tool interaction scenarios
- [ ] Test tool parameter validation and error handling
- [ ] Add conversation-based tool usage tests
- [ ] Implement automated Azure OpenAI credential detection
- [ ] Add performance benchmarks for tool invocation