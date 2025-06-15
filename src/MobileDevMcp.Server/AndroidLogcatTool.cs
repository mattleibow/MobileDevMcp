using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidLogcatTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-logcat",
        Description = "Get logcat output from Android device",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, uses first available device if not specified)"
                    },
                    "filter": {
                        "type": "string",
                        "description": "Filter logs by tag or content (optional)"
                    },
                    "level": {
                        "type": "string",
                        "description": "Minimum log level (V, D, I, W, E, F) (optional, default: I)",
                        "enum": ["V", "D", "I", "W", "E", "F"]
                    },
                    "lines": {
                        "type": "integer",
                        "description": "Number of recent log lines to retrieve (default: 100, max: 1000)"
                    },
                    "clear": {
                        "type": "boolean",
                        "description": "Clear logs before retrieving (default: false)"
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
            var filter = arguments.ContainsKey("filter") ? arguments["filter"].GetString() : null;
            var level = arguments.ContainsKey("level") ? arguments["level"].GetString() : "I";
            var lines = arguments.ContainsKey("lines") 
                ? Math.Min(arguments["lines"].GetInt32(), 1000) 
                : 100;
            var clear = arguments.ContainsKey("clear") && arguments["clear"].GetBoolean();

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

            // Clear logs if requested
            if (clear)
            {
                adb.Shell("logcat -c", targetDevice.Serial);
            }

            // Build logcat command  
            var logcatCmd = $"logcat -d -t {lines}";
            if (!string.IsNullOrEmpty(level))
            {
                logcatCmd += $" *:{level}";
            }

            // Execute logcat
            var output = adb.Shell(logcatCmd, targetDevice.Serial);
            var logOutput = output != null && output.Count > 0 
                ? string.Join("\n", output) 
                : "";
            
            if (string.IsNullOrWhiteSpace(logOutput))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"No logcat entries found on device {targetDevice.Serial}" 
                    }]
                });
            }
            
            // Apply filter if specified
            if (!string.IsNullOrEmpty(filter))
            {
                var filteredLines = logOutput.Split('\n')
                    .Where(line => line.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                if (!filteredLines.Any())
                {
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { 
                            Type = "text", 
                            Text = $"No logcat entries found matching filter '{filter}' on device {targetDevice.Serial}" 
                        }]
                    });
                }
                
                logOutput = string.Join('\n', filteredLines);
            }

            if (string.IsNullOrWhiteSpace(logOutput))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"No logcat entries found on device {targetDevice.Serial}" 
                    }]
                });
            }

            var summary = $"Logcat from device {targetDevice.Serial}";
            if (!string.IsNullOrEmpty(filter)) summary += $" (filtered by '{filter}')";
            if (clear) summary += " (logs cleared)";
            summary += $" (level: {level}, lines: {lines})";

            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"{summary}:\n\n{logOutput}" 
                }]
            });
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"Error getting logcat: {ex.Message}" 
                }]
            });
        }
    }
}