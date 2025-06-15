using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidShellTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-shell",
        Description = "Execute ADB shell commands on a connected Android device",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "command": {
                        "type": "string",
                        "description": "Shell command to execute on the device"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, will use first available device if not specified)"
                    }
                },
                "required": ["command"]
            }
            """).RootElement
    };

    public override ValueTask<CallToolResponse> InvokeAsync(
        RequestContext<CallToolRequestParams> context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract parameters from context
            var arguments = context.Params.Arguments;
            
            if (!arguments.ContainsKey("command"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = "Error: Missing required parameter 'command'."
                        }
                    ]
                });
            }

            var command = arguments["command"].GetString();
            if (string.IsNullOrEmpty(command))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = "Error: 'command' cannot be empty."
                        }
                    ]
                });
            }

            // Get optional parameters
            var deviceSerial = arguments.ContainsKey("deviceSerial") 
                ? arguments["deviceSerial"].GetString() 
                : null;

            var adb = new Adb();
            
            // If no device serial specified, get the first available device
            if (string.IsNullOrEmpty(deviceSerial))
            {
                var devices = adb.GetDevices();
                if (devices == null || devices.Count == 0)
                {
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [
                            new Content
                            {
                                Type = "text",
                                Text = "Error: No Android devices found. Please connect a device and ensure USB debugging is enabled."
                            }
                        ]
                    });
                }
                deviceSerial = devices[0].Serial;
            }

            // Execute the shell command
            var output = adb.Shell(command, deviceSerial);
            var outputText = output != null && output.Count > 0 
                ? string.Join("\n", output) 
                : "(No output)";

            var successResponse = new CallToolResponse
            {
                Content = [
                    new Content
                    {
                        Type = "text",
                        Text = $"Command executed on device {deviceSerial}:\n$ {command}\n\nOutput:\n{outputText}"
                    }
                ]
            };

            return ValueTask.FromResult(successResponse);
        }
        catch (Exception ex)
        {
            var errorResponse = new CallToolResponse
            {
                Content = [
                    new Content
                    {
                        Type = "text",
                        Text = $"Error executing shell command: {ex.Message}"
                    }
                ]
            };
            return ValueTask.FromResult(errorResponse);
        }
    }
}