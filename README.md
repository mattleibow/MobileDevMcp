# MobileDevMcp

A collection of Model Context Protocol (MCP) tools for mobile developers.

## Overview

This project provides an MCP server that offers tools useful for mobile development. Currently, it includes a basic date tool that serves as a foundation for building more sophisticated mobile development tools.

## Features

- **Date Tool**: Returns the current date in `yyyy-MM-dd` format
- **Android Tools**: A collection of tools for Android development:
  - **Android Devices List**: List all connected Android devices and emulators
  - **Android Shell**: Execute shell commands on Android devices
  - **Android Install APK**: Install APK files on connected devices
- Built with .NET 8 and the ModelContextProtocol C# SDK
- Comprehensive test coverage
- Clean architecture with separation of concerns

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

### android-devices-list

Lists all connected Android devices and emulators.

**Description**: List all connected Android devices and emulators  
**Parameters**: None  
**Response**: Text content with device information including serial numbers, states, and models

**Example Usage**:
When asked "what Android devices are connected", the tool responds with a list of connected devices.

### android-shell

Execute shell commands on connected Android devices.

**Description**: Execute a shell command on a connected Android device  
**Parameters**: 
- `command` (required): Shell command to execute
- `deviceSerial` (optional): Device serial number to target  
**Response**: Text content with command execution results

**Example Usage**:
Execute "getprop ro.build.version.release" to get Android version.

### android-install-apk

Install APK files on connected Android devices.

**Description**: Install an APK file on a connected Android device  
**Parameters**: 
- `apkPath` (required): Path to the APK file to install
- `deviceSerial` (optional): Device serial number to target  
**Response**: Text content with installation preparation details

**Example Usage**:
Install an APK file located at "/path/to/app.apk" on the connected device.

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

This is a foundation project for mobile development MCP tools. The project now includes Android development tools powered by the AdvancedSharpAdbClient package. Future enhancements could include:

- More Android SDK tools (logcat with filtering, package management, etc.)
- iOS development utilities
- Cross-platform mobile framework helpers
- Enhanced device management tools
- App store deployment helpers
- Build and deployment automation tools

## References

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Documentation](https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.html)