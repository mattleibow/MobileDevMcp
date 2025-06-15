using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidStopAvdTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-stop-avd",
        Description = "Stop a running Android Virtual Device (AVD)",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number of the AVD to stop (optional, stops first emulator if not specified)"
                    },
                    "force": {
                        "type": "boolean",
                        "description": "Force stop the AVD (default: false)"
                    }
                },
                "required": []
            }
            """).RootElement
    };

    public override ValueTask<CallToolResponse> InvokeAsync(
        RequestContext<CallToolRequestParams> context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var arguments = context?.Params?.Arguments ?? new Dictionary<string, JsonElement>();
            
            var deviceSerial = arguments.ContainsKey("deviceSerial") ? arguments["deviceSerial"].GetString() : null;
            var force = arguments.ContainsKey("force") && arguments["force"].GetBoolean();

            var adb = new Adb();
            var devices = adb.GetDevices();

            if (devices == null || !devices.Any())
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "No Android devices found." }]
                });
            }

            // Find emulator to stop
            var targetDevice = string.IsNullOrEmpty(deviceSerial) 
                ? devices.FirstOrDefault(d => d.IsEmulator)
                : devices.FirstOrDefault(d => d.Serial == deviceSerial && d.IsEmulator);

            if (targetDevice == null)
            {
                var message = string.IsNullOrEmpty(deviceSerial) 
                    ? "No running emulators found."
                    : $"Emulator with serial '{deviceSerial}' not found or is not an emulator.";
                
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = message }]
                });
            }

            // Stop the emulator using shell commands
            var output = adb.Shell("reboot -p", targetDevice.Serial);
            var outputText = output != null && output.Count > 0 
                ? string.Join("\n", output) 
                : "";
            
            // Since the emulator will shut down, the command might not return a response
            // If no error occurred, assume success
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"Successfully stopped AVD {targetDevice.Serial}" 
                }]
            });
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"Error stopping AVD: {ex.Message}" 
                }]
            });
        }
    }
}