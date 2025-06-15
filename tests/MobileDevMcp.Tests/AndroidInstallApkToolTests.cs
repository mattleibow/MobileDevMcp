using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class AndroidInstallApkToolTests
{
    [Fact]
    public void AndroidInstallApkTool_ProtocolTool_HasCorrectName()
    {
        // Arrange
        var tool = new AndroidInstallApkTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("android-install-apk", protocolTool.Name);
    }

    [Fact]
    public void AndroidInstallApkTool_ProtocolTool_HasCorrectDescription()
    {
        // Arrange
        var tool = new AndroidInstallApkTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.Equal("Install an APK file to a connected Android device", protocolTool.Description);
    }

    [Fact]
    public void AndroidInstallApkTool_ProtocolTool_HasValidInputSchema()
    {
        // Arrange
        var tool = new AndroidInstallApkTool();

        // Act
        var protocolTool = tool.ProtocolTool;

        // Assert
        Assert.NotEqual(default(JsonElement), protocolTool.InputSchema);
        
        // Verify the schema has required properties
        var schemaString = protocolTool.InputSchema.GetRawText();
        var schema = JsonDocument.Parse(schemaString);
        
        Assert.Equal("object", schema.RootElement.GetProperty("type").GetString());
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("apkPath", out _));
        Assert.True(schema.RootElement.GetProperty("required").EnumerateArray().Any(r => r.GetString() == "apkPath"));
    }

    [Fact]
    public void AndroidInstallApkTool_PromptExample_InstallApkToDevice()
    {
        // This test simulates a user asking "install this APK to my device"
        // Arrange
        var tool = new AndroidInstallApkTool();

        // Act - simulate what would happen when called through MCP
        var toolName = tool.ProtocolTool.Name;
        var toolDescription = tool.ProtocolTool.Description;
        
        // Assert the tool is properly configured for this use case
        Assert.Equal("android-install-apk", toolName);
        Assert.Equal("Install an APK file to a connected Android device", toolDescription);
        
        // Verify the tool requires apkPath parameter
        var schema = tool.ProtocolTool.InputSchema;
        var schemaDoc = JsonDocument.Parse(schema.GetRawText());
        var requiredParams = schemaDoc.RootElement.GetProperty("required").EnumerateArray()
            .Select(r => r.GetString()).ToList();
        Assert.Contains("apkPath", requiredParams);
    }
}