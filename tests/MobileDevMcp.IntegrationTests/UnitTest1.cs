using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using MobileDevMcp.Server;

namespace MobileDevMcp.IntegrationTests;

public class McpAiIntegrationTests : IDisposable
{
    private readonly IHost _host;
    private readonly IChatClient? _chatClient;
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
        var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4";
        
        _hasAzureOpenAiConfig = !string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey);

        if (_hasAzureOpenAiConfig)
        {
            // Create AI client with Azure OpenAI - this will be implemented later
            // For now, we'll focus on testing the MCP server directly
            _chatClient = null;
        }
        else
        {
            _chatClient = null;
        }
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

    [Fact(Skip = "Azure OpenAI integration not yet implemented")]
    public async Task AiClient_CanUseTools_WhenConfigured()
    {
        // Skip this test until we implement AI integration
        // This will be the main integration test that uses IChatClient with MCP tools
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _host?.Dispose();
    }
}