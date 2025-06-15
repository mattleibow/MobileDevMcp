using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class AndroidDevicesToolTests
{
    [Fact]
    public void AndroidDevicesTool_ProtocolTool_HasCorrectName()
    {
        // Arrange
        var tool = new AndroidDevicesTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("android-devices", protocolTool.Name);
    }

    [Fact]
    public void AndroidDevicesTool_ProtocolTool_HasCorrectDescription()
    {
        // Arrange
        var tool = new AndroidDevicesTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("List connected Android devices and emulators", protocolTool.Description);
    }

    [Fact]
    public void AndroidDevicesTool_ProtocolTool_HasValidInputSchema()
    {
        // Arrange
        var tool = new AndroidDevicesTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.NotEqual(default(JsonElement), protocolTool.InputSchema);
        
        // Verify the schema is a valid JSON object with expected structure
        var schemaString = protocolTool.InputSchema.GetRawText();
        var expectedSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {},
                "required": []
            }
            """);
        
        Assert.Equal(expectedSchema.RootElement.GetRawText(), schemaString);
    }

    [Fact]
    public void AndroidDevicesTool_ReturnsCorrectResponseFormat()
    {
        // Arrange
        var tool = new AndroidDevicesTool();

        // Act - This will either return devices or a no devices message
        // Note: This test may return an error if Android SDK is not installed, which is expected
        var response = tool.InvokeAsync(null!, CancellationToken.None).Result;

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.Single(response.Content);
        Assert.Equal("text", response.Content[0].Type);
        Assert.NotNull(response.Content[0].Text);
        
        // Response should either contain device info, helpful message, or error
        var responseText = response.Content[0].Text;
        Assert.True(
            responseText.Contains("Found") || 
            responseText.Contains("No Android devices found") ||
            responseText.Contains("Error"),
            $"Unexpected response format: {responseText}");
    }

    [Fact]
    public void AndroidDevicesTool_PromptExample_ListAndroidDevices()
    {
        // This test simulates a user asking "list my android devices"
        // Arrange
        var tool = new AndroidDevicesTool();

        // Act - simulate what would happen when called through MCP
        var toolName = tool.ProtocolTool.Name;
        var toolDescription = tool.ProtocolTool.Description;
        
        // Assert the tool is properly configured for this use case
        Assert.Equal("android-devices", toolName);
        Assert.Equal("List connected Android devices and emulators", toolDescription);
        
        // For this test, we only verify tool metadata, not actual execution
        // since Android SDK may not be available in test environment
        Assert.NotNull(tool.ProtocolTool.InputSchema);
    }
}