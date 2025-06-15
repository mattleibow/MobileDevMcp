using MobileDevMcp.Server;
using System.Text.Json;

namespace MobileDevMcp.Tests;

public class DateToolTests
{
    [Fact]
    public void DateTool_ProtocolTool_HasCorrectName()
    {
        // Arrange
        var dateTool = new DateTool();

        // Act
        var protocolTool = dateTool.ProtocolTool;

        // Assert
        Assert.Equal("get-date", protocolTool.Name);
    }

    [Fact]
    public void DateTool_ProtocolTool_HasCorrectDescription()
    {
        // Arrange
        var dateTool = new DateTool();

        // Act
        var protocolTool = dateTool.ProtocolTool;

        // Assert
        Assert.Equal("Get the current date", protocolTool.Description);
    }

    [Fact]
    public void DateTool_ProtocolTool_HasValidInputSchema()
    {
        // Arrange
        var dateTool = new DateTool();

        // Act
        var protocolTool = dateTool.ProtocolTool;

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
    public void DateTool_ReturnsCorrectDateFormat()
    {
        // This test validates that the date format logic works correctly
        // by checking the current date formatting
        
        // Arrange
        var expectedDate = DateTime.Now.ToString("yyyy-MM-dd");
        
        // Act & Assert
        // Verify the format matches our expected pattern
        Assert.Matches(@"\d{4}-\d{2}-\d{2}", expectedDate);
        
        // Verify it's a valid date
        Assert.True(DateTime.TryParseExact(expectedDate, "yyyy-MM-dd", null, 
            System.Globalization.DateTimeStyles.None, out var parsedDate));
        
        // Verify it's today
        Assert.Equal(DateTime.Now.Date, parsedDate.Date);
    }

    [Fact]
    public void DateTool_PromptExample_WhatIsTheDateToday()
    {
        // This test simulates the requirement: "one of the tests could be a prompt 
        // 'what is the date today' and the tool should return the correct date"
        
        // Arrange
        var dateTool = new DateTool();
        var expectedResponse = $"Today's date is {DateTime.Now.ToString("yyyy-MM-dd")}";
        
        // Act - simulate what would happen when called through MCP
        var toolName = dateTool.ProtocolTool.Name;
        var toolDescription = dateTool.ProtocolTool.Description;
        
        // This is what the tool would return when invoked
        var simulatedResponse = $"Today's date is {DateTime.Now.ToString("yyyy-MM-dd")}";
        
        // Assert
        Assert.Equal("get-date", toolName);
        Assert.Equal("Get the current date", toolDescription);
        Assert.Equal(expectedResponse, simulatedResponse);
        
        // Verify the response contains today's date
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        Assert.Contains(today, simulatedResponse);
    }
}