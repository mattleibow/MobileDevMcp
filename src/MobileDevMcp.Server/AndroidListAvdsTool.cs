using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;
using AndroidSdk;

namespace MobileDevMcp.Server;

public class AndroidListAvdsTool : McpServerTool
{
    public override Tool ProtocolTool => new()
    {
        Name = "android-list-avds",
        Description = "List available Android Virtual Devices (AVDs)",
        InputSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {},
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
            var avdManager = new AvdManager();
            var avds = avdManager.ListAvds();

            if (avds == null || !avds.Any())
            {
                return ValueTask.FromResult(new CallToolResponse
                {
                    Content = [
                        new Content
                        {
                            Type = "text",
                            Text = "No Android Virtual Devices (AVDs) found. You can create AVDs using:\n" +
                                   "- Android Studio AVD Manager\n" +
                                   "- Command line: avdmanager create avd -n <name> -k <systemImage>"
                        }
                    ]
                });
            }

            var avdList = string.Join("\n", avds.Select((avd, index) => 
            {
                var avdInfo = $"{index + 1}. Name: {avd.Name}";
                if (!string.IsNullOrEmpty(avd.Target))
                    avdInfo += $", Target: {avd.Target}";
                if (!string.IsNullOrEmpty(avd.Device))
                    avdInfo += $", Device: {avd.Device}";
                if (!string.IsNullOrEmpty(avd.BasedOn))
                    avdInfo += $", Based on: {avd.BasedOn}";
                return avdInfo;
            }));

            var successResponse = new CallToolResponse
            {
                Content = [
                    new Content
                    {
                        Type = "text",
                        Text = $"Found {avds.Count()} Android Virtual Device(s):\n\n{avdList}"
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
                        Text = $"Error listing AVDs: {ex.Message}"
                    }
                ]
            };
            return ValueTask.FromResult(errorResponse);
        }
    }
}