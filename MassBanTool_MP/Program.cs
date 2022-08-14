using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace MassBanToolMP
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (OperatingSystem.IsLinux())
            {
                // Check if there is a GUI.
                if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")))
                {
                    throw new PlatformNotSupportedException("No GUI Session detected cannot start GUI: XDG_CURRENT_DESKTOP doesn't exists/has no value.");
                }
            }
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
        }
    }
}
