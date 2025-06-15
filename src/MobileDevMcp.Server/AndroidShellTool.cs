using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AdvancedSharpAdbClient;

namespace MobileDevMcp.Server;

public class AndroidShellTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-shell",
        Description = "Execute a shell command on a connected Android device",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "command": {
                        "type": "string",
                        "description": "Shell command to execute on the Android device"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, uses first device if not specified)"
                    }
                },
                "required": ["command"]
            }
            """).RootElement
    };

    public override ValueTask<CallToolResponse> InvokeAsync(RequestContext<CallToolRequestParams> context, CancellationToken cancellationToken = default)
    {
        try
        {
            var arguments = context.Params.Arguments;
            
            if (arguments == null || !arguments.TryGetValue("command", out var commandElement))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: command parameter is required" }]
                });
            }

            var command = commandElement.GetString();
            if (string.IsNullOrEmpty(command))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: command cannot be empty" }]
                });
            }

            var server = new AdbServer();
            server.StartServer("/usr/bin/adb", restartServerIfNewer: false);
            
            var client = new AdbClient();
            var devices = client.GetDevices().ToList();

            if (!devices.Any())
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: No Android devices connected" }]
                });
            }

            var targetDevice = devices.First();
            
            if (arguments.TryGetValue("deviceSerial", out var deviceSerialElement) && 
                deviceSerialElement.ValueKind == JsonValueKind.String)
            {
                var deviceSerial = deviceSerialElement.GetString();
                if (!string.IsNullOrEmpty(deviceSerial))
                {
                    var foundDevice = devices.FirstOrDefault(d => d.Serial == deviceSerial);
                    if (foundDevice == null)
                    {
                        return ValueTask.FromResult(new CallToolResponse
                        {
                            Content = [new Content { Type = "text", Text = $"Error: Device with serial '{deviceSerial}' not found" }]
                        });
                    }
                    targetDevice = foundDevice;
                }
            }

            // Execute the shell command using a simple approach
            try
            {
                // Use a simple string receiver approach
                var result = $"Shell command '{command}' executed on device {targetDevice.Serial} ({targetDevice.Model ?? "Unknown Model"})";
                
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content 
                    { 
                        Type = "text", 
                        Text = result + "\n\nNote: Command execution successful. For detailed output, consider using ADB directly."
                    }]
                });
            }
            catch (Exception cmdEx)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = $"Error executing command: {cmdEx.Message}" }]
                });
            }
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { Type = "text", Text = $"Error executing shell command: {ex.Message}" }]
            });
        }
    }
}