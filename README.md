# MobileDevMcp

A collection of Model Context Protocol (MCP) tools for mobile developers.

## Overview

This project provides an MCP server that offers tools useful for mobile development. Currently, it includes a basic date tool that serves as a foundation for building more sophisticated mobile development tools.

## Features

- **Date Tool**: Returns the current date in `yyyy-MM-dd` format
- **Android Development Tools**: Comprehensive set of Android development utilities
  - **Device Management**: List and interact with connected Android devices and emulators
  - **App Installation**: Install and manage APK files on devices
  - **Shell Commands**: Execute ADB shell commands remotely
  - **Package Management**: List and filter installed packages
  - **Virtual Device Management**: List available Android Virtual Devices (AVDs)
- Built with .NET 8 and the ModelContextProtocol C# SDK
- Comprehensive test coverage
- Clean architecture with separation of concerns
- AndroidSdk NuGet package integration for robust Android tooling

## Project Structure

```
├── src/
│   ├── MobileDevMcp.Server/     # Core MCP server library
│   └── MobileDevMcp.Host/       # Console host application
├── tests/
│   └── MobileDevMcp.Tests/      # Unit tests
└── MobileDevMcp.sln             # Solution file
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running the MCP Server

```bash
dotnet run --project src/MobileDevMcp.Host
```

## Available Tools

### get-date

Returns the current date.

**Description**: Get the current date  
**Parameters**: None  
**Response**: Text content with today's date in format "Today's date is yyyy-MM-dd"

**Example Usage**:
When asked "what is the date today", the tool responds with "Today's date is 2025-01-15" (or the current date).

### android-devices

List connected Android devices and emulators.

**Description**: List connected Android devices and emulators  
**Parameters**: None  
**Response**: List of connected devices with their serial numbers, products, models, and emulator status

**Example Usage**:
When asked "list my android devices", the tool responds with details of all connected Android devices and emulators.

### android-install-apk

Install an APK file to a connected Android device.

**Description**: Install an APK file to a connected Android device  
**Parameters**:
- `apkPath` (required): Path to the APK file to install
- `deviceSerial` (optional): Device serial number (uses first available if not specified)
- `reinstall` (optional): Whether to reinstall if app is already installed (default: false)

**Response**: Success or error message with installation details

**Example Usage**:
"Install this APK /path/to/app.apk to my device" - installs the specified APK to the first available device.

### android-shell

Execute ADB shell commands on a connected Android device.

**Description**: Execute ADB shell commands on a connected Android device  
**Parameters**:
- `command` (required): Shell command to execute on the device
- `deviceSerial` (optional): Device serial number (uses first available if not specified)

**Response**: Command output from the device

**Example Usage**:
"Run 'getprop ro.build.version.release' on my android device" - executes the shell command and returns the Android version.

### android-list-packages

List installed packages on a connected Android device.

**Description**: List installed packages on a connected Android device  
**Parameters**:
- `deviceSerial` (optional): Device serial number (uses first available if not specified)
- `includeUninstalled` (optional): Include uninstalled packages (default: false)
- `filter` (optional): Filter packages by name containing this string

**Response**: List of installed packages (limited to first 50 for readability)

**Example Usage**:
"List packages containing 'google' on my device" - shows all installed packages with 'google' in their name.

### android-list-avds

List available Android Virtual Devices (AVDs).

**Description**: List available Android Virtual Devices (AVDs)  
**Parameters**: None  
**Response**: List of available AVDs with their names, targets, devices, and configuration details

**Example Usage**:
"Show my android emulators" - displays all configured Android Virtual Devices available for use.

## Development

### Adding New Tools

To add a new MCP tool:

1. Create a new class that inherits from `McpServerTool`
2. Override the `ProtocolTool` property to define the tool metadata
3. Implement the `InvokeAsync` method with your tool logic
4. Register the tool in the DI container in `Program.cs`
5. Add comprehensive tests

### Testing

The project includes comprehensive unit tests that validate:
- Tool metadata (name, description, schema)
- Correct response format
- Date formatting and accuracy
- Synchronous execution for simple tools

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Contributing

This is a foundation project for mobile development MCP tools. Future enhancements could include:

- Android SDK tools (build, deploy, debug)
- iOS development utilities
- Cross-platform mobile framework helpers
- Device management tools
- App store deployment helpers

## References

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Documentation](https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.html)