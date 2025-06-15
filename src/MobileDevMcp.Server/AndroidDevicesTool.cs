using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidDevicesTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-devices",
        Description = "List connected Android devices and emulators",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {},
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
            var adb = new Adb();
            var devices = adb.GetDevices();
            
            if (devices == null || devices.Count == 0)
            {
                var response = new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = "No Android devices found. Make sure:\n" +
                                   "- Android SDK is installed\n" +
                                   "- ADB is in your PATH\n" +
                                   "- USB debugging is enabled on your device\n" +
                                   "- Device is connected via USB or network"
                        }
                    ]
                };
                return ValueTask.FromResult(response);
            }

            var deviceList = string.Join("\n", devices.Select((device, index) => 
            {
                var deviceInfo = $"{index + 1}. Serial: {device.Serial}";
                if (!string.IsNullOrEmpty(device.Product))
                    deviceInfo += $", Product: {device.Product}";
                if (!string.IsNullOrEmpty(device.Model))
                    deviceInfo += $", Model: {device.Model}";
                if (device.IsEmulator)
                    deviceInfo += " (Emulator)";
                return deviceInfo;
            }));

            var successResponse = new CallToolResponse
            {
                Content = [
                    new Content
                    {
                        Type = "text",
                        Text = $"Found {devices.Count} Android device(s):\n\n{deviceList}"
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
                        Text = $"Error listing Android devices: {ex.Message}"
                    }
                ]
            };
            return ValueTask.FromResult(errorResponse);
        }
    }
}