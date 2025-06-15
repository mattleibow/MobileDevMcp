using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AdvancedSharpAdbClient;
using System.Text;

namespace MobileDevMcp.Server;

public class AndroidDevicesListTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-devices-list",
        Description = "List all connected Android devices and emulators",
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
        try
        {
            var server = new AdbServer();
            var startResult = server.StartServer("/usr/bin/adb", restartServerIfNewer: false);
            
            var client = new AdbClient();
            var devices = client.GetDevices();

            var responseText = new StringBuilder();
            responseText.AppendLine("Connected Android Devices:");
            
            if (!devices.Any())
            {
                responseText.AppendLine("No devices connected.");
            }
            else
            {
                foreach (var device in devices)
                {
                    responseText.AppendLine($"- {device.Serial} ({device.State}) - {device.Model ?? "Unknown Model"}");
                }
            }

            var response = new CallToolResponse
            {
                Content =
                [
                    new Content
                    {
                        Type = "text",
                        Text = responseText.ToString()
                    }
                ]
            };

            return ValueTask.FromResult(response);
        }
        catch (Exception ex)
        {
            var errorResponse = new CallToolResponse
            {
                Content =
                [
                    new Content
                    {
                        Type = "text",
                        Text = $"Error listing Android devices: {ex.Message}"
                    }
                ]
            };

            return ValueTask.FromResult(errorResponse);
        }
    }
}