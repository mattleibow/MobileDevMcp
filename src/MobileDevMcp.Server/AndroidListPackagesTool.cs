using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidListPackagesTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-list-packages",
        Description = "List installed packages on a connected Android device",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "deviceSerial": {
                        "type": "string",
                        "description": "Device serial number (optional, will use first available device if not specified)"
                    },
                    "includeUninstalled": {
                        "type": "boolean",
                        "description": "Include uninstalled packages (default: false)"
                    },
                    "filter": {
                        "type": "string",
                        "description": "Filter packages by name containing this string (optional)"
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
            // Extract parameters from context
            var arguments = context?.Params?.Arguments ?? new Dictionary<string, JsonElement>();
            
            // Get optional parameters
            var deviceSerial = arguments.ContainsKey("deviceSerial") 
                ? arguments["deviceSerial"].GetString() 
                : null;
            
            var includeUninstalled = arguments.ContainsKey("includeUninstalled") 
                ? arguments["includeUninstalled"].GetBoolean() 
                : false;
                
            var filter = arguments.ContainsKey("filter") 
                ? arguments["filter"].GetString() 
                : null;

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

            // Create PackageManager instance for the device
            var packageManager = new PackageManager
            {
                AdbSerial = deviceSerial
            };

            // List packages
            var packages = packageManager.ListPackages(
                includeUninstalled: includeUninstalled,
                showState: PackageManager.PackageListState.All,
                showSource: PackageManager.PackageSourceType.All
            );

            if (packages == null || packages.Count == 0)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = $"No packages found on device {deviceSerial}"
                        }
                    ]
                });
            }

            // Apply filter if specified
            var filteredPackages = packages;
            if (!string.IsNullOrEmpty(filter))
            {
                filteredPackages = packages.Where(p => p.PackageName.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var packageList = string.Join("\n", filteredPackages.Take(50).Select(p => p.PackageName)); // Limit to first 50 packages
            var totalCount = filteredPackages.Count;
            var displayedCount = Math.Min(50, totalCount);

            var responseText = $"Found {totalCount} package(s) on device {deviceSerial}";
            if (!string.IsNullOrEmpty(filter))
            {
                responseText += $" (filtered by '{filter}')";
            }
            responseText += $":\n\nShowing {displayedCount} packages:\n{packageList}";
            
            if (totalCount > 50)
            {
                responseText += $"\n\n... and {totalCount - 50} more packages. Use filter to narrow results.";
            }

            var successResponse = new CallToolResponse
            {
                Content = [
                    new Content
                    {
                        Type = "text",
                        Text = responseText
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
                        Text = $"Error listing packages: {ex.Message}"
                    }
                ]
            };
            return ValueTask.FromResult(errorResponse);
        }
    }
}