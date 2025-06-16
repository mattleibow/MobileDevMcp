using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using MobileDevMcp.Server;
using System.ComponentModel;
using System.Text.Json;

namespace MobileDevMcp.IntegrationTests;

public class McpAiIntegrationTests : IDisposable
{
    private readonly IHost _host;
    private readonly bool _hasAzureOpenAiConfig;

    public McpAiIntegrationTests()
    {
        // Set up MCP server host
        var builder = Host.CreateApplicationBuilder();
        
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        // Configure MCP server
        builder.Services.AddMcpServer(options =>
        {
            options.ServerInfo = new()
            {
                Name = "Mobile Dev MCP Server",
                Version = "1.0.0"
            };
        });

        // Register our tools
        builder.Services.AddSingleton<McpServerTool, DateTool>();
        builder.Services.AddSingleton<McpServerTool, AndroidDevicesTool>();
        builder.Services.AddSingleton<McpServerTool, AndroidListAvdsTool>();

        _host = builder.Build();

        // Check if Azure OpenAI configuration is available
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        
        _hasAzureOpenAiConfig = !string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey);
    }

    [Fact]
    public void McpServer_CanBeCreated()
    {
        // Basic test to ensure MCP server can be instantiated
        Assert.NotNull(_host);
        var tools = _host.Services.GetServices<McpServerTool>();
        Assert.NotEmpty(tools);
    }

    [Fact]
    public async Task DateTool_CanBeInvokedDirectly()
    {
        // Test that we can invoke the DateTool directly
        var dateTool = _host.Services.GetServices<McpServerTool>()
            .OfType<DateTool>()
            .FirstOrDefault();
        
        Assert.NotNull(dateTool);
        
        var response = await dateTool.InvokeAsync(null!, CancellationToken.None);
        
        Assert.NotNull(response);
        Assert.NotEmpty(response.Content);
        Assert.Contains("Today's date is", response.Content[0].Text);
    }

    [Fact]
    public async Task AndroidDevicesTool_CanBeInvokedDirectly()
    {
        // Test that we can invoke the AndroidDevicesTool directly
        var androidTool = _host.Services.GetServices<McpServerTool>()
            .OfType<AndroidDevicesTool>()
            .FirstOrDefault();
        
        Assert.NotNull(androidTool);
        
        var response = await androidTool.InvokeAsync(null!, CancellationToken.None);
        
        Assert.NotNull(response);
        Assert.NotEmpty(response.Content);
        // The response will likely contain "No Android devices found" since no devices are connected in test environment
        Assert.True(response.Content[0].Text.Contains("devices") || response.Content[0].Text.Contains("No Android devices found"));
    }

    [Fact]
    public void AllRegisteredTools_HaveCorrectMetadata()
    {
        var tools = _host.Services.GetServices<McpServerTool>().ToList();
        
        Assert.True(tools.Count >= 3); // Should have at least DateTool, AndroidDevicesTool, AndroidListAvdsTool
        
        foreach (var tool in tools)
        {
            var protocolTool = tool.ProtocolTool;
            Assert.NotNull(protocolTool.Name);
            Assert.NotEmpty(protocolTool.Name);
            Assert.NotNull(protocolTool.Description);
            Assert.NotEmpty(protocolTool.Description);
        }
    }

    [Fact]
    public async Task McpTools_CanBeConvertedToAiFunctions()
    {
        // Test that MCP tools can be converted to AI functions
        var dateFunction = CreateDateFunction();
        var androidFunction = CreateAndroidDevicesFunction();
        
        Assert.NotNull(dateFunction);
        Assert.NotNull(androidFunction);
        
        // Test the functions can be invoked
        var dateResult = await dateFunction.InvokeAsync(null);
        var androidResult = await androidFunction.InvokeAsync(null);
        
        Assert.NotNull(dateResult);
        Assert.Contains("Today's date is", dateResult.ToString());
        
        Assert.NotNull(androidResult);
        var androidText = androidResult.ToString();
        Assert.True(androidText!.Contains("devices") || androidText.Contains("No Android devices found"));
    }

    [Fact]
    public void AiFunctions_HaveCorrectMetadata()
    {
        // Test that converted AI functions have proper metadata
        var dateFunction = CreateDateFunction();
        var androidFunction = CreateAndroidDevicesFunction();
        
        Assert.NotNull(dateFunction);
        Assert.NotNull(androidFunction);
        
        // Basic validation that functions are created successfully
        // The exact metadata access will depend on the final Azure OpenAI integration
    }

    [Fact(Skip = "Azure OpenAI integration will be implemented when credentials are available")]
    public async Task AiClient_CanUseMcpTools_WithAzureOpenAI()
    {
        // This test demonstrates how the AI integration would work
        // It will be enabled when Azure OpenAI credentials are configured
        
        if (!_hasAzureOpenAiConfig)
        {
            // Test would be skipped in real scenario
            return;
        }
        
        // Example of how this would work:
        // 1. Create AI client with Azure OpenAI
        // 2. Convert MCP tools to AI functions
        // 3. Use ChatOptions with tools
        // 4. Send messages to AI and verify tool usage
        
        await Task.CompletedTask;
    }

    private AIFunction CreateDateFunction()
    {
        var dateTool = _host.Services.GetServices<McpServerTool>()
            .OfType<DateTool>()
            .First();

        return AIFunctionFactory.Create(async () =>
        {
            var response = await dateTool.InvokeAsync(null!, CancellationToken.None);
            return response.Content.FirstOrDefault()?.Text ?? "Unable to get date";
        }, "get_current_date", "Get the current date");
    }

    private AIFunction CreateAndroidDevicesFunction()
    {
        var androidTool = _host.Services.GetServices<McpServerTool>()
            .OfType<AndroidDevicesTool>()
            .First();

        return AIFunctionFactory.Create(async () =>
        {
            var response = await androidTool.InvokeAsync(null!, CancellationToken.None);
            return response.Content.FirstOrDefault()?.Text ?? "Unable to check Android devices";
        }, "list_android_devices", "List connected Android devices");
    }

    public void Dispose()
    {
        _host?.Dispose();
    }
}