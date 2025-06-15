using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidCreateAvdTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-create-avd",
        Description = "Create a new Android Virtual Device (AVD)",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "Name for the new AVD"
                    },
                    "package": {
                        "type": "string", 
                        "description": "System image package (e.g., 'system-images;android-33;google_apis;x86_64')"
                    },
                    "device": {
                        "type": "string",
                        "description": "Device definition (optional, defaults to 'pixel')"
                    },
                    "force": {
                        "type": "boolean",
                        "description": "Force creation if AVD already exists (default: false)"
                    }
                },
                "required": ["name", "package"]
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
            
            if (!arguments.ContainsKey("name"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'name' parameter is required" }]
                });
            }

            if (!arguments.ContainsKey("package"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'package' parameter is required" }]
                });
            }

            var name = arguments["name"].GetString()!;
            var package = arguments["package"].GetString()!;
            var device = arguments.ContainsKey("device") ? arguments["device"].GetString() : "pixel";
            var force = arguments.ContainsKey("force") && arguments["force"].GetBoolean();

            var avdManager = new AvdManager();
            
            // Check if AVD already exists
            var existingAvds = avdManager.ListAvds();
            if (existingAvds?.Any(avd => avd.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == true && !force)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Error: AVD '{name}' already exists. Use force=true to overwrite." 
                    }]
                });
            }

            // Create the AVD using avdmanager command through shell
            var adb = new Adb();
            var avdManagerCmd = force 
                ? $"avdmanager create avd -n {name} -k {package} -d {device} --force"
                : $"avdmanager create avd -n {name} -k {package} -d {device}";
            
            // Try to execute the command (this may not work if avdmanager is not in PATH)
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "avdmanager";
                process.StartInfo.Arguments = force 
                    ? $"create avd -n {name} -k {package} -d {device} --force"
                    : $"create avd -n {name} -k {package} -d {device}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0)
                {
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"Successfully created AVD '{name}' with package '{package}' and device '{device}'" 
                        }]
                    });
                }
                else
                {
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"Failed to create AVD '{name}': {error}" 
                        }]
                    });
                }
            }
            catch (Exception cmdEx)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Failed to create AVD '{name}'. Please ensure Android SDK tools are installed and in PATH. Error: {cmdEx.Message}" 
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
                    Text = $"Error creating AVD: {ex.Message}" 
                }]
            });
        }
    }
}