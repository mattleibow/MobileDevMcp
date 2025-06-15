using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidLaunchAppTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-launch-app",
        Description = "Launch an app on a connected Android device by package name",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "packageName": {
                        "type": "string",
                        "description": "Package name of the app to launch (e.g., 'com.android.settings')"
                    },
                    "activityName": {
                        "type": "string",
                        "description": "Specific activity to launch (optional, launches main activity if not specified)"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, uses first available device if not specified)"
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
            var activityName = arguments.ContainsKey("activityName") ? arguments["activityName"].GetString() : null;
            var deviceSerial = arguments.ContainsKey("deviceSerial") ? arguments["deviceSerial"].GetString() : null;

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

            // Launch the app using shell commands
            var command = string.IsNullOrEmpty(activityName) 
                ? $"monkey -p {packageName} -c android.intent.category.LAUNCHER 1"
                : $"am start -n {packageName}/{activityName}";

            var output = adb.Shell(command, targetDevice.Serial);
            var outputText = output != null && output.Count > 0 
                ? string.Join("\n", output) 
                : "";
            
            // Check if launch was successful (monkey returns success, am start shows activity launch)
            var success = outputText.Contains("Events injected") || outputText.Contains("Starting:");
            
            if (success || string.IsNullOrEmpty(outputText))
            {
                var launchTarget = string.IsNullOrEmpty(activityName) ? packageName : $"{packageName}/{activityName}";
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Successfully launched '{launchTarget}' on device {targetDevice.Serial}" 
                    }]
                });
            }
            else
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Failed to launch '{packageName}' on device {targetDevice.Serial}: {outputText}" 
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
                    Text = $"Error launching app: {ex.Message}" 
                }]
            });
        }
    }
}