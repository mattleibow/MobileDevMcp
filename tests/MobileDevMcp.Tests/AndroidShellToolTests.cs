using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class AndroidShellToolTests
{
    [Fact]
    public void AndroidShellTool_ProtocolTool_HasCorrectName()
    {
        // Arrange
        var tool = new AndroidShellTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("android-shell", protocolTool.Name);
    }

    [Fact]
    public void AndroidShellTool_ProtocolTool_HasCorrectDescription()
    {
        // Arrange
        var tool = new AndroidShellTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("Execute a shell command on a connected Android device", protocolTool.Description);
    }

    [Fact]
    public void AndroidShellTool_ProtocolTool_HasValidInputSchema()
    {
        // Arrange
        var tool = new AndroidShellTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.NotEqual(default(JsonElement), protocolTool.InputSchema);
        
        // Verify the schema is a valid JSON object with expected structure
        var schemaString = protocolTool.InputSchema.GetRawText();
        var schema = JsonDocument.Parse(schemaString);
        
        // Verify it has the required structure
        Assert.Equal("object", schema.RootElement.GetProperty("type").GetString());
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("command", out _));
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("deviceSerial", out _));
        
        var requiredArray = schema.RootElement.GetProperty("required");
        Assert.Equal(1, requiredArray.GetArrayLength());
        Assert.Equal("command", requiredArray[0].GetString());
    }

    [Fact]
    public void AndroidShellTool_RequiredParameters_Validation()
    {
        // This test validates that the required parameters are correctly defined
        
        // Arrange
        var tool = new AndroidShellTool();
        var protocolTool = tool.ProtocolTool;
        var schema = JsonDocument.Parse(protocolTool.InputSchema.GetRawText());
        
        // Act & Assert
        // Verify command is required
        var requiredArray = schema.RootElement.GetProperty("required");
        var requiredFields = new List<string>();
        foreach (var item in requiredArray.EnumerateArray())
        {
            requiredFields.Add(item.GetString()!);
        }
        
        Assert.Contains("command", requiredFields);
        Assert.DoesNotContain("deviceSerial", requiredFields); // deviceSerial is optional
    }

    [Fact]
    public void AndroidShellTool_PromptExample_ExecuteCommand()
    {
        // This test simulates the requirement for executing shell commands
        
        // Arrange
        var tool = new AndroidShellTool();
        
        // Act - simulate what would happen when called through MCP
        var toolName = tool.ProtocolTool.Name;
        var toolDescription = tool.ProtocolTool.Description;
        
        // Assert
        Assert.Equal("android-shell", toolName);
        Assert.Equal("Execute a shell command on a connected Android device", toolDescription);
        
        // Verify the response would contain command execution information
        Assert.Contains("shell", toolDescription.ToLower());
        Assert.Contains("command", toolDescription.ToLower());
    }
}