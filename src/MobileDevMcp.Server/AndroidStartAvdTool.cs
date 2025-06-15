using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidStartAvdTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-start-avd",
        Description = "Start an Android Virtual Device (AVD)",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "Name of the AVD to start"
                    },
                    "noWindow": {
                        "type": "boolean",
                        "description": "Start AVD without graphical window (headless mode, default: false)"
                    },
                    "wipeData": {
                        "type": "boolean", 
                        "description": "Wipe user data before starting (default: false)"
                    }
                },
                "required": ["name"]
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

            var name = arguments["name"].GetString()!;
            var noWindow = arguments.ContainsKey("noWindow") && arguments["noWindow"].GetBoolean();
            var wipeData = arguments.ContainsKey("wipeData") && arguments["wipeData"].GetBoolean();

            var avdManager = new AvdManager();
            
            // Check if AVD exists
            var existingAvds = avdManager.ListAvds();
            if (existingAvds?.Any(avd => avd.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) != true)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Error: AVD '{name}' not found. Use android-list-avds to see available AVDs." 
                    }]
                });
            }

            // Start the AVD using emulator command
            try
            {
                var args = new List<string> { "@" + name };
                if (noWindow) args.Add("-no-window");
                if (wipeData) args.Add("-wipe-data");
                
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "emulator";
                process.StartInfo.Arguments = string.Join(" ", args);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                
                // Don't wait for the process as emulator runs in background
                var options = new List<string>();
                if (noWindow) options.Add("headless mode");
                if (wipeData) options.Add("wiped data");
                
                var optionsText = options.Any() ? $" ({string.Join(", ", options)})" : "";
                
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Successfully started AVD '{name}'{optionsText}. It may take a few moments to fully boot." 
                    }]
                });
            }
            catch (Exception ex)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Failed to start AVD '{name}'. Please ensure Android SDK tools are installed and in PATH. Error: {ex.Message}" 
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
                    Text = $"Error starting AVD: {ex.Message}" 
                }]
            });
        }
    }
}