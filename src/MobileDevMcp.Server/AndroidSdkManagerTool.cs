using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidSdkManagerTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-sdk-manager",
        Description = "Manage Android SDK packages and components",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "action": {
                        "type": "string",
                        "description": "Action to perform",
                        "enum": ["list", "install", "update", "uninstall"]
                    },
                    "package": {
                        "type": "string",
                        "description": "Package name to install/uninstall (required for install/uninstall actions)"
                    },
                    "filter": {
                        "type": "string",
                        "description": "Filter packages by name (optional, for list action)"
                    },
                    "includeObsolete": {
                        "type": "boolean",
                        "description": "Include obsolete packages in list (default: false)"
                    },
                    "acceptLicenses": {
                        "type": "boolean",
                        "description": "Automatically accept SDK licenses (default: false)"
                    }
                },
                "required": ["action"]
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
            
            if (!arguments.ContainsKey("action"))
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = "Error: 'action' parameter is required" }]
                });
            }

            var action = arguments["action"].GetString()!.ToLowerInvariant();
            var package = arguments.ContainsKey("package") ? arguments["package"].GetString() : null;
            var filter = arguments.ContainsKey("filter") ? arguments["filter"].GetString() : null;
            var includeObsolete = arguments.ContainsKey("includeObsolete") && arguments["includeObsolete"].GetBoolean();
            var acceptLicenses = arguments.ContainsKey("acceptLicenses") && arguments["acceptLicenses"].GetBoolean();

            // Most SDK management operations need to be done via command line
            // as the AndroidSdk package has limited SDK manager capabilities
            switch (action)
            {
                case "list":
                    return ListPackagesViaCommand(filter, includeObsolete);
                    
                case "install":
                    if (string.IsNullOrEmpty(package))
                    {
                        return ValueTask.FromResult(new CallToolResponse
                        {
                            Content = [new Content { Type = "text", Text = "Error: 'package' parameter is required for install action" }]
                        });
                    }
                    return InstallPackageViaCommand(package, acceptLicenses);
                    
                case "update":
                    return UpdatePackagesViaCommand(acceptLicenses);
                    
                case "uninstall":
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { Type = "text", Text = "Uninstall action is not supported. Use Android Studio or manually delete SDK components." }]
                    });
                    
                default:
                    return ValueTask.FromResult(new CallToolResponse
                    {
                        Content = [new Content { Type = "text", Text = $"Error: Unknown action '{action}'. Use: list, install, or update" }]
                    });
            }
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"Error managing SDK: {ex.Message}" 
                }]
            });
        }
    }

    private ValueTask<CallToolResponse> ListPackagesViaCommand(string? filter, bool includeObsolete)
    {
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "sdkmanager";
            process.StartInfo.Arguments = includeObsolete ? "--list --include_obsolete" : "--list";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode != 0)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = $"Error listing SDK packages: {error}" }]
                });
            }

            var lines = output.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            
            if (!string.IsNullOrEmpty(filter))
            {
                lines = lines.Where(line => line.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!lines.Any())
            {
                var message = string.IsNullOrEmpty(filter) 
                    ? "No SDK packages found" 
                    : $"No SDK packages found matching filter '{filter}'";
                
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { Type = "text", Text = message }]
                });
            }

            var packageList = string.Join("\n", lines.Take(50)); // Limit to 50 lines
            
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { 
                    Type = "text", 
                    Text = $"SDK packages (showing first 50 entries):\n\n{packageList}" 
                }]
            });
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { Type = "text", Text = $"Error listing packages: {ex.Message}. Ensure Android SDK tools are installed and in PATH." }]
            });
        }
    }

    private ValueTask<CallToolResponse> InstallPackageViaCommand(string package, bool acceptLicenses)
    {
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "sdkmanager";
            process.StartInfo.Arguments = acceptLicenses ? $"\"{package}\"" : $"--no-https \"{package}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            if (acceptLicenses)
            {
                process.StartInfo.RedirectStandardInput = true;
            }
            
            process.Start();
            
            if (acceptLicenses)
            {
                // Auto-accept licenses by sending "y" responses
                for (int i = 0; i < 10; i++)
                {
                    process.StandardInput.WriteLine("y");
                }
                process.StandardInput.Close();
            }
            
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Successfully installed SDK package '{package}'" 
                    }]
                });
            }
            else
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Failed to install SDK package '{package}': {error}" 
                    }]
                });
            }
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { Type = "text", Text = $"Error installing package: {ex.Message}. Ensure Android SDK tools are installed and in PATH." }]
            });
        }
    }

    private ValueTask<CallToolResponse> UpdatePackagesViaCommand(bool acceptLicenses)
    {
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "sdkmanager";
            process.StartInfo.Arguments = "--update";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            if (acceptLicenses)
            {
                process.StartInfo.RedirectStandardInput = true;
            }
            
            process.Start();
            
            if (acceptLicenses)
            {
                // Auto-accept licenses by sending "y" responses
                for (int i = 0; i < 10; i++)
                {
                    process.StandardInput.WriteLine("y");
                }
                process.StandardInput.Close();
            }
            
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = "Successfully updated all SDK packages" 
                    }]
                });
            }
            else
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [new Content { 
                        Type = "text", 
                        Text = $"Failed to update SDK packages: {error}" 
                    }]
                });
            }
        }
        catch (Exception ex)
        {
            return ValueTask.FromResult(new CallToolResponse
            {
                Content = [new Content { Type = "text", Text = $"Error updating packages: {ex.Message}. Ensure Android SDK tools are installed and in PATH." }]
            });
        }
    }
}