using Avalonia;
using CopilotChatExporter.UI;

namespace CopilotChatExporter.Mobile;

public static class MobileAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
