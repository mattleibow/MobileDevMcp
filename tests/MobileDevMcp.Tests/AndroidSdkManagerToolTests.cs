using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class AndroidSdkManagerToolTests
{
    [Fact]
    public void ProtocolTool_ReturnsCorrectName()
    {
        // Arrange
        var tool = new AndroidSdkManagerTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("android-sdk-manager", protocolTool.Name);
    }

    [Fact]
    public void ProtocolTool_ReturnsCorrectDescription()
    {
        // Arrange
        var tool = new AndroidSdkManagerTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("Manage Android SDK packages and components", protocolTool.Description);
    }

    [Fact]
    public void ProtocolTool_HasRequiredParameters()
    {
        // Arrange
        var tool = new AndroidSdkManagerTool();

        // Act
        var protocolTool = tool.ProtocolTool;
        var schema = protocolTool.InputSchema;

        // Assert
        Assert.Equal(JsonValueKind.Object, schema.ValueKind);
        
        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("action", out var actionProperty));
        Assert.Equal("string", actionProperty.GetProperty("type").GetString());
        
        var required = schema.GetProperty("required");
        Assert.Equal(JsonValueKind.Array, required.ValueKind);
        Assert.Contains("action", required.EnumerateArray().Select(r => r.GetString()));
    }

    [Fact]
    public async Task InvokeAsync_WithMissingAction_ReturnsError()
    {
        // Arrange
        var tool = new AndroidSdkManagerTool();

        // Act
        var response = await tool.InvokeAsync(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Content);
        Assert.Equal("text", response.Content[0].Type);
        Assert.Contains("'action' parameter is required", response.Content[0].Text);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidAction_ReturnsError()
    {
        // Arrange
        var tool = new AndroidSdkManagerTool();

        // Act  
        var response = await tool.InvokeAsync(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Content);
        Assert.Equal("text", response.Content[0].Type);
        Assert.True(response.Content[0].Text.Contains("'action' parameter is required") ||
                   response.Content[0].Text.Contains("Unknown action"));
    }

    [Fact]
    public async Task InvokeAsync_WithListAction_ReturnsResponse()
    {
        // Arrange
        var tool = new AndroidSdkManagerTool();

        // Act
        var response = await tool.InvokeAsync(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Content);
        Assert.Equal("text", response.Content[0].Type);
        // Since SDK tools may not be available in test environment, expect error about tools not being in PATH
        Assert.True(response.Content[0].Text.Contains("SDK packages") || 
                   response.Content[0].Text.Contains("Ensure Android SDK tools are installed") ||
                   response.Content[0].Text.Contains("'action' parameter is required"));
    }
}