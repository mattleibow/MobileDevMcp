# GitHub Copilot Instructions for MobileDevMcp

## Project Overview

This is a Model Context Protocol (MCP) server designed for mobile developers, built with .NET 8 and the official ModelContextProtocol C# SDK. The project provides a foundation for mobile development tools through MCP.

## Architecture & Structure

### Solution Organization
```
├── src/
│   ├── MobileDevMcp.Server/     # Core MCP server library containing tools
│   └── MobileDevMcp.Host/       # Console host application with DI setup
├── tests/
│   └── MobileDevMcp.Tests/      # Comprehensive unit tests
└── MobileDevMcp.sln             # Solution file
```

### Technology Stack
- **.NET 8** with nullable reference types enabled
- **ModelContextProtocol SDK v0.2.0-preview.3** (official MCP C# SDK)
- **Microsoft.Extensions.Hosting** for dependency injection and application hosting
- **xUnit** for testing with comprehensive coverage
- **System.Text.Json** for JSON handling

## Key Patterns & Conventions

### MCP Tool Implementation Pattern
All MCP tools should follow this pattern:

```csharp
public class YourTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "kebab-case-name",
        Description = "Clear, concise description",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object", 
                "properties": { /* define parameters */ },
                "required": [ /* required params */ ]
            }
            """).RootElement
    };

    public override ValueTask<CallToolResponse> InvokeAsync(
        RequestContext<CallToolRequestParams> context, 
        CancellationToken cancellationToken = default)
    {
        // Tool implementation
        var response = new CallToolResponse
        {
            Content = [
                new Content 
                { 
                    Type = "text", 
                    Text = "Response text" 
                }
            ]
        };
        return ValueTask.FromResult(response);
    }
}
```

### Dependency Injection Registration
Register new tools in `src/MobileDevMcp.Host/Program.cs`:

```csharp
builder.Services.AddSingleton<McpServerTool, YourNewTool>();
```

### Testing Conventions
Follow comprehensive testing pattern covering:
- Tool metadata validation (name, description, schema)
- Input/output format validation
- Business logic correctness
- Specific user scenario tests

Test file naming: `{ToolName}Tests.cs`

## Code Style & Standards

### General Guidelines
- Use nullable reference types (`<Nullable>enable</Nullable>`)
- Prefer `ValueTask<T>` over `Task<T>` for potentially synchronous operations
- Use `JsonDocument.Parse()` for static JSON schema definitions
- Follow async/await patterns even for synchronous operations via `ValueTask.FromResult()`
- Use implicit usings where appropriate

### Naming Conventions
- **Tool names**: Use kebab-case (e.g., "get-date", "build-android")
- **Classes**: PascalCase with "Tool" suffix (e.g., `DateTool`, `AndroidBuildTool`)
- **Test classes**: `{ClassName}Tests`
- **Namespaces**: Follow project structure (`MobileDevMcp.Server`, `MobileDevMcp.Tests`)

### JSON Schema Patterns
- Always include `"type": "object"` as root
- Use `"properties": {}` and `"required": []` arrays
- Define clear, descriptive property names
- Include validation constraints where appropriate

## Mobile Development Focus

This project is specifically designed for mobile developers. When adding new tools, prioritize:

### Android Development
- Build tools (Gradle, Android SDK)
- Device management (ADB commands)
- APK deployment and debugging
- Android-specific utilities

### iOS Development  
- Xcode project management
- iOS device management
- App Store deployment
- iOS-specific utilities

### Cross-Platform Frameworks
- Xamarin/.NET MAUI tools
- Flutter helpers
- React Native utilities
- Cordova/PhoneGap support

### General Mobile Development
- App store deployment workflows
- Device testing and debugging
- Performance analysis tools
- Mobile CI/CD helpers

## Error Handling & Validation

### Input Validation
- Always validate tool inputs against the defined JSON schema
- Provide clear error messages for invalid inputs
- Use appropriate HTTP status codes for MCP responses

### Exception Handling
- Catch and handle tool-specific exceptions gracefully
- Return meaningful error messages in MCP response format
- Log errors appropriately for debugging

## Testing Strategy

### Test Coverage Requirements
- **Metadata tests**: Validate tool name, description, and schema
- **Format tests**: Verify input/output format compliance
- **Business logic tests**: Test core functionality
- **Integration tests**: Test actual tool execution scenarios
- **Edge case tests**: Handle invalid inputs and error conditions

### Test Data Management
- Use consistent test data across similar tools
- Mock external dependencies (file system, network, etc.)
- Test both success and failure scenarios

## Performance Considerations

### Tool Performance
- Keep tool execution lightweight and fast
- Use synchronous execution for simple operations
- Consider async patterns for I/O intensive operations
- Cache results where appropriate

### Memory Management
- Dispose of resources properly
- Avoid memory leaks in long-running scenarios
- Use `ValueTask` for better memory efficiency

## Dependencies & Packages

### Core Dependencies
- Keep dependencies minimal and focused
- Prefer Microsoft packages for .NET functionality
- Use official MCP SDK packages only
- Avoid unnecessary third-party dependencies

### Version Management
- Keep all projects on same .NET version (currently .NET 8)
- Pin MCP SDK version for consistency
- Update dependencies carefully with testing

## Build & Deployment

### Build Commands
```bash
dotnet build                    # Build entire solution
dotnet test                     # Run all tests
dotnet run --project src/MobileDevMcp.Host  # Run MCP server
```

### CI/CD Considerations
- All tests must pass before merging
- Build should be clean with no warnings
- Consider code coverage thresholds
- Validate against multiple .NET versions if needed

## Future Expansion Guidelines

When adding new tools:

1. **Follow established patterns** from existing tools like `DateTool`
2. **Add comprehensive tests** following the existing test structure
3. **Update README.md** with new tool documentation
4. **Consider mobile development workflows** - how will this help mobile developers?
5. **Keep tools focused** - each tool should have a single, clear responsibility
6. **Document usage examples** - provide clear examples of tool usage

## Common Patterns to Follow

### Tool Response Format
Always return responses in this format:
```csharp
new CallToolResponse
{
    Content = [
        new Content { Type = "text", Text = "Your response" }
    ]
}
```

### Error Response Pattern
For error scenarios:
```csharp
new CallToolResponse
{
    Content = [
        new Content { Type = "text", Text = "Error: Clear error message" }
    ],
    IsError = true  // If available in SDK
}
```

### Async Pattern
Even for synchronous operations, maintain async signature:
```csharp
public override ValueTask<CallToolResponse> InvokeAsync(...)
{
    // Synchronous work
    var result = DoSynchronousWork();
    return ValueTask.FromResult(response);
}
```

This project serves as a foundation for comprehensive mobile development tooling through the Model Context Protocol. Follow these patterns and guidelines to maintain consistency and quality across all tools.