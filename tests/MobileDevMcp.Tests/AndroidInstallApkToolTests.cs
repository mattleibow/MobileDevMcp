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
        Assert.Equal("Install an APK file on a connected Android device", protocolTool.Description);
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
        
        // Verify the schema is a valid JSON object with expected structure
        var schemaString = protocolTool.InputSchema.GetRawText();
        var schema = JsonDocument.Parse(schemaString);
        
        // Verify it has the required structure
        Assert.Equal("object", schema.RootElement.GetProperty("type").GetString());
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("apkPath", out _));
        Assert.True(schema.RootElement.GetProperty("properties").TryGetProperty("deviceSerial", out _));
        
        var requiredArray = schema.RootElement.GetProperty("required");
        Assert.Equal(1, requiredArray.GetArrayLength());
        Assert.Equal("apkPath", requiredArray[0].GetString());
    }

    [Fact]
    public void AndroidInstallApkTool_RequiredParameters_Validation()
    {
        // This test validates that the required parameters are correctly defined
        
        // Arrange
        var tool = new AndroidInstallApkTool();
        var protocolTool = tool.ProtocolTool;
        var schema = JsonDocument.Parse(protocolTool.InputSchema.GetRawText());
        
        // Act & Assert
        // Verify apkPath is required
        var requiredArray = schema.RootElement.GetProperty("required");
        var requiredFields = new List<string>();
        foreach (var item in requiredArray.EnumerateArray())
        {
            requiredFields.Add(item.GetString()!);
        }
        
        Assert.Contains("apkPath", requiredFields);
        Assert.DoesNotContain("deviceSerial", requiredFields); // deviceSerial is optional
    }

    [Fact]
    public void AndroidInstallApkTool_PromptExample_InstallApk()
    {
        // This test simulates the requirement for installing APK files
        
        // Arrange
        var tool = new AndroidInstallApkTool();
        
        // Act - simulate what would happen when called through MCP
        var toolName = tool.ProtocolTool.Name;
        var toolDescription = tool.ProtocolTool.Description;
        
        // Assert
        Assert.Equal("android-install-apk", toolName);
        Assert.Equal("Install an APK file on a connected Android device", toolDescription);
        
        // Verify the response would contain APK installation information
        Assert.Contains("apk", toolDescription.ToLower());
        Assert.Contains("install", toolDescription.ToLower());
    }
}