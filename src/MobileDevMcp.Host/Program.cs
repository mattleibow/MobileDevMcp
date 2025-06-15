using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using MobileDevMcp.Server;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure MCP server
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new()
    {
        Name = "Mobile Dev MCP Server",
        Version = "1.0.0"
    };
});

// Register our tools
builder.Services.AddSingleton<McpServerTool, DateTool>();

// Android tools
builder.Services.AddSingleton<McpServerTool, AndroidDevicesListTool>();
builder.Services.AddSingleton<McpServerTool, AndroidShellTool>();
builder.Services.AddSingleton<McpServerTool, AndroidInstallApkTool>();

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting Mobile Dev MCP Server...");

// Run the MCP server
await host.RunAsync();
