using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidInstallApkTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-install-apk",
        Description = "Install an APK file to a connected Android device",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "apkPath": {
                        "type": "string",
                        "description": "Path to the APK file to install"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, will use first available device if not specified)"
                    },
                    "reinstall": {
                        "type": "boolean",
                        "description": "Whether to reinstall if app is already installed (default: false)"
                    }
                },
                "required": ["apkPath"]
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
            var arguments = context?.Params?.Arguments ?? new Dictionary<string, JsonElement>();
            
            if (!arguments.ContainsKey("apkPath"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = "Error: Missing required parameter 'apkPath'."
                        }
                    ]
                });
            }

            var apkPath = arguments["apkPath"].GetString();
            if (string.IsNullOrEmpty(apkPath))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = "Error: 'apkPath' cannot be empty."
                        }
                    ]
                });
            }

            // Check if APK file exists
            var apkFile = new FileInfo(apkPath);
            if (!apkFile.Exists)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = $"Error: APK file not found at path: {apkPath}"
                        }
                    ]
                });
            }

            // Get optional parameters
            var deviceSerial = arguments.ContainsKey("deviceSerial") 
                ? arguments["deviceSerial"].GetString() 
                : null;
            
            var reinstall = arguments.ContainsKey("reinstall") 
                ? arguments["reinstall"].GetBoolean() 
                : false;

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

            // Prepare install options
            var installOptions = new Adb.AdbInstallOptions
            {
                Replace = reinstall
            };

            // Install the APK
            adb.Install(apkFile, installOptions, deviceSerial);

            var successResponse = new CallToolResponse
            {
                Content = [
                    new Content
                    {
                        Type = "text",
                        Text = $"Successfully installed APK '{apkFile.Name}' to device {deviceSerial}"
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
                        Text = $"Error installing APK: {ex.Message}"
                    }
                ]
            };
            return ValueTask.FromResult(errorResponse);
        }
    }
}