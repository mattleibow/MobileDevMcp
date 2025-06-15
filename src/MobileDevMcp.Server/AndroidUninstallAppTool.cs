using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidUninstallAppTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-uninstall-app",
        Description = "Uninstall an app from a connected Android device by package name",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "packageName": {
                        "type": "string",
                        "description": "Package name of the app to uninstall (e.g., 'com.example.myapp')"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, uses first available device if not specified)"
                    },
                    "keepData": {
                        "type": "boolean",
                        "description": "Keep app data and cache when uninstalling (default: false)"
                    }
                },
                "required": ["packageName"]
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
            
            if (!arguments.ContainsKey("packageName"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'packageName' parameter is required" }]
                });
            }

            var packageName = arguments["packageName"].GetString()!;
            var deviceSerial = arguments.ContainsKey("deviceSerial") ? arguments["deviceSerial"].GetString() : null;
            var keepData = arguments.ContainsKey("keepData") && arguments["keepData"].GetBoolean();

            var adb = new Adb();
            var devices = adb.GetDevices();

            if (devices == null || !devices.Any())
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "No Android devices found. Please connect a device or start an emulator." }]
                });
            }

            var targetDevice = string.IsNullOrEmpty(deviceSerial) 
                ? devices.First() 
                : devices.FirstOrDefault(d => d.Serial == deviceSerial);

            if (targetDevice == null)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = $"Device with serial '{deviceSerial}' not found." }]
                });
            }

            // Check if package is installed using PackageManager
            var packageManager = new PackageManager
            {
                AdbSerial = targetDevice.Serial
            };
            var packages = packageManager.ListPackages(
                includeUninstalled: false,
                showState: PackageManager.PackageListState.All,
                showSource: PackageManager.PackageSourceType.All
            );
            
            if (!packages.Any(p => p.PackageName == packageName))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Package '{packageName}' is not installed on device {targetDevice.Serial}" 
                    }]
                });
            }

            // Uninstall the app using shell command
            var uninstallCmd = keepData ? $"pm uninstall -k {packageName}" : $"pm uninstall {packageName}";
            var output = adb.Shell(uninstallCmd, targetDevice.Serial);
            var outputText = output != null && output.Count > 0 
                ? string.Join("\n", output) 
                : "";
            
            if (outputText.Contains("Success"))
            {
                var dataText = keepData ? " (data preserved)" : " (data removed)";
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Successfully uninstalled '{packageName}' from device {targetDevice.Serial}{dataText}" 
                    }]
                });
            }
            else
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Failed to uninstall '{packageName}' from device {targetDevice.Serial}: {outputText}" 
                    }]
                });
            }
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"Error uninstalling app: {ex.Message}" 
                }]
            });
        }
    }
}