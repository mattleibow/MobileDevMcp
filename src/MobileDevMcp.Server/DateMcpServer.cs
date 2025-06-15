using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace MobileDevMcp.Server;

public class DateTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "get-date",
        Description = "Get the current date",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {},
                "required": []
            }
            """).RootElement
    };

    public override ValueTask<CallToolResponse> InvokeAsync(RequestContext<CallToolRequestParams> context, CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        
        var response = new CallToolResponse
        {
            Content =
            [
                new Content
                {
                    Type = "text",
                    Text = $"Today's date is {currentDate}"
                }
            ]
        };

        return ValueTask.FromResult(response);
    }
}
