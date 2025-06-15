using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class AndroidListAvdsToolTests
{
    [Fact]
    public void AndroidListAvdsTool_ProtocolTool_HasCorrectName()
    {
        // Arrange
        var tool = new AndroidListAvdsTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("android-list-avds", protocolTool.Name);
    }

    [Fact]
    public void AndroidListAvdsTool_ProtocolTool_HasCorrectDescription()
    {
        // Arrange
        var tool = new AndroidListAvdsTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("List available Android Virtual Devices (AVDs)", protocolTool.Description);
    }

    [Fact]
    public void AndroidListAvdsTool_ProtocolTool_HasValidInputSchema()
    {
        // Arrange
        var tool = new AndroidListAvdsTool();

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
    public void AndroidListAvdsTool_PromptExample_ListAvailableEmulators()
    {
        // This test simulates a user asking "list my android emulators"
        // Arrange
        var tool = new AndroidListAvdsTool();

        // Act - simulate what would happen when called through MCP
        var toolName = tool.ProtocolTool.Name;
        var toolDescription = tool.ProtocolTool.Description;
        
        // Assert the tool is properly configured for this use case
        Assert.Equal("android-list-avds", toolName);
        Assert.Equal("List available Android Virtual Devices (AVDs)", toolDescription);
        
        // Verify the tool has no required parameters (should work without arguments)
        var schema = tool.ProtocolTool.InputSchema;
        var schemaDoc = JsonDocument.Parse(schema.GetRawText());
        var requiredParams = schemaDoc.RootElement.GetProperty("required").EnumerateArray().ToList();
        Assert.Empty(requiredParams);
    }
}