# MobileDevMcp

A collection of Model Context Protocol (MCP) tools for mobile developers.

## Overview

This project provides an MCP server that offers tools useful for mobile development. Currently, it includes a basic date tool that serves as a foundation for building more sophisticated mobile development tools.

## Features

- **Date Tool**: Returns the current date in `yyyy-MM-dd` format
- **Comprehensive Android Development Tools**: Complete set of Android development utilities
  - **Device Management**: List and interact with connected Android devices and emulators
  - **App Management**: Install, launch, and uninstall APK files on devices
  - **File Operations**: Push and pull files to/from Android devices
  - **Virtual Device Management**: Create, start, stop, and list Android Virtual Devices (AVDs)
  - **SDK Management**: Install, update, and list Android SDK packages
  - **Shell Commands**: Execute ADB shell commands remotely
  - **Package Management**: List and filter installed packages
  - **Logging**: Access device logcat with filtering capabilities
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

### android-create-avd

Create a new Android Virtual Device (AVD).

**Description**: Create a new Android Virtual Device (AVD)  
**Parameters**:
- `name` (required): Name for the new AVD
- `package` (required): System image package (e.g., 'system-images;android-33;google_apis;x86_64')
- `device` (optional): Device definition (default: 'pixel')
- `force` (optional): Force creation if AVD already exists (default: false)

**Response**: Success or error message with creation details

**Example Usage**:
"Create AVD named 'test_device' with API 33" - creates new emulator with specified configuration.

### android-start-avd

Start an Android Virtual Device (AVD).

**Description**: Start an Android Virtual Device (AVD)  
**Parameters**:
- `name` (required): Name of the AVD to start
- `noWindow` (optional): Start AVD without graphical window (headless mode, default: false)
- `wipeData` (optional): Wipe user data before starting (default: false)

**Response**: Success or error message

**Example Usage**:
"Start AVD 'test_device' in headless mode" - boots the emulator without UI.

### android-stop-avd

Stop a running Android Virtual Device (AVD).

**Description**: Stop a running Android Virtual Device (AVD)  
**Parameters**:
- `deviceSerial` (optional): Device serial number of the AVD to stop (stops first emulator if not specified)
- `force` (optional): Force stop the AVD (default: false)

**Response**: Success or error message

**Example Usage**:
"Stop the running emulator" - shuts down the currently running AVD.

### android-launch-app

Launch an app on a connected Android device by package name.

**Description**: Launch an app on a connected Android device by package name  
**Parameters**:
- `packageName` (required): Package name of the app to launch (e.g., 'com.android.settings')
- `activityName` (optional): Specific activity to launch (launches main activity if not specified)
- `deviceSerial` (optional): Device serial number (uses first available device if not specified)

**Response**: Success or error message

**Example Usage**:
"Launch Settings app on my device" - opens the Android Settings application.

### android-uninstall-app

Uninstall an app from a connected Android device by package name.

**Description**: Uninstall an app from a connected Android device by package name  
**Parameters**:
- `packageName` (required): Package name of the app to uninstall (e.g., 'com.example.myapp')
- `deviceSerial` (optional): Device serial number (uses first available device if not specified)
- `keepData` (optional): Keep app data and cache when uninstalling (default: false)

**Response**: Success or error message

**Example Usage**:
"Uninstall com.example.app from my device" - removes the specified application.

### android-push-file

Push a file from local system to Android device.

**Description**: Push a file from local system to Android device  
**Parameters**:
- `localPath` (required): Local file path to push
- `remotePath` (required): Remote path on device (e.g., '/sdcard/myfile.txt')
- `deviceSerial` (optional): Device serial number (uses first available device if not specified)

**Response**: Success or error message with file transfer details

**Example Usage**:
"Push myfile.txt to /sdcard/ on my device" - copies local file to device storage.

### android-pull-file

Pull a file from Android device to local system.

**Description**: Pull a file from Android device to local system  
**Parameters**:
- `remotePath` (required): Remote file path on device (e.g., '/sdcard/myfile.txt')
- `localPath` (required): Local path to save the file
- `deviceSerial` (optional): Device serial number (uses first available device if not specified)

**Response**: Success or error message with file transfer details

**Example Usage**:
"Pull /sdcard/screenshot.png to my desktop" - downloads file from device to local system.

### android-logcat

Get logcat output from Android device.

**Description**: Get logcat output from Android device  
**Parameters**:
- `deviceSerial` (optional): Device serial number (uses first available device if not specified)
- `filter` (optional): Filter logs by tag or content
- `level` (optional): Minimum log level (V, D, I, W, E, F) (default: I)
- `lines` (optional): Number of recent log lines to retrieve (default: 100, max: 1000)
- `clear` (optional): Clear logs before retrieving (default: false)

**Response**: Filtered logcat output

**Example Usage**:
"Show error logs from my device" - displays recent error-level log entries.

### android-sdk-manager

Manage Android SDK packages and components.

**Description**: Manage Android SDK packages and components  
**Parameters**:
- `action` (required): Action to perform ('list', 'install', 'update')
- `package` (optional): Package name to install (required for install action)
- `filter` (optional): Filter packages by name (for list action)
- `includeObsolete` (optional): Include obsolete packages in list (default: false)
- `acceptLicenses` (optional): Automatically accept SDK licenses (default: false)

**Response**: Action results or package listings

**Example Usage**:
"List available SDK packages" - shows all available Android SDK components for installation.

## Complete Workflow Examples

### Setting Up a New Development Environment

```bash
# 1. List and install SDK packages
android-sdk-manager --action="list" --filter="platform"
android-sdk-manager --action="install" --package="platforms;android-33" --acceptLicenses=true
android-sdk-manager --action="install" --package="system-images;android-33;google_apis;x86_64" --acceptLicenses=true

# 2. Create and start an AVD
android-create-avd --name="dev_device" --package="system-images;android-33;google_apis;x86_64"
android-start-avd --name="dev_device"

# 3. Verify setup
android-devices
android-list-avds
```

### App Development and Testing Workflow

```bash
# 1. Install your app
android-install-apk --apkPath="/path/to/myapp.apk" --reinstall=true

# 2. Launch the app
android-launch-app --packageName="com.example.myapp"

# 3. Monitor logs during testing
android-logcat --filter="MyApp" --level="D" --lines=200

# 4. Debug shell commands
android-shell --command="dumpsys activity activities"

# 5. Pull generated files from device
android-pull-file --remotePath="/sdcard/myapp_data.json" --localPath="./debug_data.json"

# 6. Clean up when done
android-uninstall-app --packageName="com.example.myapp"
```

### Device Management

```bash
# List all connected devices and emulators
android-devices

# Check device properties
android-shell --command="getprop ro.build.version.release"
android-shell --command="getprop ro.product.model"

# Manage files
android-push-file --localPath="./test_data.txt" --remotePath="/sdcard/test_data.txt"
android-pull-file --remotePath="/sdcard/DCIM/Camera/" --localPath="./photos/"

# Monitor system logs
android-logcat --level="W" --clear=true --lines=100
```

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

This project provides a comprehensive foundation for mobile development MCP tools with extensive Android support. Current capabilities include:

**✅ Implemented Android Features:**
- Complete device and emulator management
- SDK package management and installation
- AVD creation, startup, and management
- App installation, launching, and uninstalling
- File transfer operations (push/pull)
- Real-time logging and debugging
- Shell command execution
- Package listing and filtering

**🚀 Future Enhancement Opportunities:**
- **iOS Development Support**: Xcode project management, iOS device management, iOS Simulator tools
- **Cross-Platform Frameworks**: Enhanced Xamarin/.NET MAUI tools, Flutter helpers, React Native utilities
- **Advanced Android Features**: Gradle build integration, Android signing tools, Play Store deployment
- **Development Workflow**: CI/CD pipeline helpers, automated testing tools, performance profiling
- **Multi-Device Testing**: Device farm integration, parallel testing capabilities

**📱 Mobile-First Design:**
This project is specifically designed for mobile developers who need seamless integration between development tools and AI assistants. All tools follow consistent patterns and provide comprehensive error handling for real-world development scenarios.

## References

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Documentation](https://modelcontextprotocol.github.io/csharp-sdk/api/ModelContextProtocol.html)