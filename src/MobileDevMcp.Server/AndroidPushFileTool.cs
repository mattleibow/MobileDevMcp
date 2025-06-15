using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidPushFileTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-push-file",
        Description = "Push a file from local system to Android device",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "localPath": {
                        "type": "string",
                        "description": "Local file path to push"
                    },
                    "remotePath": {
                        "type": "string", 
                        "description": "Remote path on device (e.g., '/sdcard/myfile.txt')"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, uses first available device if not specified)"
                    }
                },
                "required": ["localPath", "remotePath"]
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
            
            if (!arguments.ContainsKey("localPath"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'localPath' parameter is required" }]
                });
            }

            if (!arguments.ContainsKey("remotePath"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'remotePath' parameter is required" }]
                });
            }

            var localPath = arguments["localPath"].GetString()!;
            var remotePath = arguments["remotePath"].GetString()!;
            var deviceSerial = arguments.ContainsKey("deviceSerial") ? arguments["deviceSerial"].GetString() : null;

            // Check if local file exists
            if (!File.Exists(localPath))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = $"Error: Local file '{localPath}' not found" }]
                });
            }

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

            // Push the file using shell command (since Push API signature is unclear)
            var pushCmd = $"push \"{localPath}\" \"{remotePath}\"";
            var output = adb.Shell($"echo 'Attempting file push...'", targetDevice.Serial);
            
            // Use alternative approach through Android SDK command line
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "adb";
                process.StartInfo.Arguments = $"-s {targetDevice.Serial} push \"{localPath}\" \"{remotePath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                
                var cmdOutput = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    var fileInfo = new FileInfo(localPath);
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"Successfully pushed '{localPath}' ({fileInfo.Length} bytes) to '{remotePath}' on device {targetDevice.Serial}" 
                        }]
                    });
                }
                else
                {
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"Failed to push file to device {targetDevice.Serial}: {error}" 
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
                        Text = $"Failed to push file to device {targetDevice.Serial}: {ex.Message}" 
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
                    Text = $"Error pushing file: {ex.Message}" 
                }]
            });
        }
    }
}