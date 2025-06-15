using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidPullFileTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-pull-file",
        Description = "Pull a file from Android device to local system",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "remotePath": {
                        "type": "string",
                        "description": "Remote file path on device (e.g., '/sdcard/myfile.txt')"
                    },
                    "localPath": {
                        "type": "string", 
                        "description": "Local path to save the file"
                    },
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, uses first available device if not specified)"
                    }
                },
                "required": ["remotePath", "localPath"]
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
            
            if (!arguments.ContainsKey("remotePath"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'remotePath' parameter is required" }]
                });
            }

            if (!arguments.ContainsKey("localPath"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'localPath' parameter is required" }]
                });
            }

            var remotePath = arguments["remotePath"].GetString()!;
            var localPath = arguments["localPath"].GetString()!;
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

            // Check if remote file exists
            var checkOutput = adb.Shell($"test -f {remotePath} && echo 'exists' || echo 'not found'", targetDevice.Serial);
            var checkText = checkOutput != null && checkOutput.Count > 0 
                ? string.Join("", checkOutput).Trim()
                : "";
                
            if (!checkText.Contains("exists"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = $"Error: Remote file '{remotePath}' not found on device {targetDevice.Serial}" }]
                });
            }

            // Create local directory if it doesn't exist
            var localDir = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(localDir) && !Directory.Exists(localDir))
            {
                Directory.CreateDirectory(localDir);
            }

            // Pull the file using shell command (since Pull API signature is unclear)
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "adb";
                process.StartInfo.Arguments = $"-s {targetDevice.Serial} pull \"{remotePath}\" \"{localPath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                
                var cmdOutput = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0 && File.Exists(localPath))
                {
                    var fileInfo = new FileInfo(localPath);
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"Successfully pulled '{remotePath}' from device {targetDevice.Serial} to '{localPath}' ({fileInfo.Length} bytes)" 
                        }]
                    });
                }
                else
                {
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"Failed to pull file from device {targetDevice.Serial}: {error}" 
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
                        Text = $"Failed to pull file from device {targetDevice.Serial}: {ex.Message}" 
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
                    Text = $"Error pulling file: {ex.Message}" 
                }]
            });
        }
    }
}