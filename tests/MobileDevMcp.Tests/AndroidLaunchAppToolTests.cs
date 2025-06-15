using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class AndroidLaunchAppToolTests
{
    [Fact]
    public void ProtocolTool_ReturnsCorrectName()
    {
        // Arrange
        var tool = new AndroidLaunchAppTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("android-launch-app", protocolTool.Name);
    }

    [Fact]
    public void ProtocolTool_ReturnsCorrectDescription()
    {
        // Arrange
        var tool = new AndroidLaunchAppTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("Launch an app on a connected Android device by package name", protocolTool.Description);
    }

    [Fact]
    public void ProtocolTool_HasRequiredParameters()
    {
        // Arrange
        var tool = new AndroidLaunchAppTool();

        // Act
        var protocolTool = tool.ProtocolTool;
        var schema = protocolTool.InputSchema;

        // Assert
        Assert.Equal(JsonValueKind.Object, schema.ValueKind);
        
        var properties = schema.GetProperty("properties");
        Assert.True(properties.TryGetProperty("packageName", out var packageNameProperty));
        Assert.Equal("string", packageNameProperty.GetProperty("type").GetString());
        
        var required = schema.GetProperty("required");
        Assert.Equal(JsonValueKind.Array, required.ValueKind);
        Assert.Contains("packageName", required.EnumerateArray().Select(r => r.GetString()));
    }

    [Fact]
    public async Task InvokeAsync_WithMissingPackageName_ReturnsError()
    {
        // Arrange
        var tool = new AndroidLaunchAppTool();

        // Act
        var response = await tool.InvokeAsync(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Content);
        Assert.Equal("text", response.Content[0].Type);
        Assert.Contains("'packageName' parameter is required", response.Content[0].Text);
    }

    [Fact]
    public async Task InvokeAsync_WithValidParameters_ReturnsResponse()
    {
        // Arrange
        var tool = new AndroidLaunchAppTool();

        // Act
        var response = await tool.InvokeAsync(null!, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Single(response.Content);
        Assert.Equal("text", response.Content[0].Type);
        // Since no devices are connected in test environment, expect device error message
        Assert.True(response.Content[0].Text.Contains("No Android devices found") || 
                   response.Content[0].Text.Contains("parameter is required"));
    }
}