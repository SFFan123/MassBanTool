using Avalonia;
using Avalonia.ReactiveUI;
using System;
using Avalonia.Media;

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

        public static Color Yellow { get; } = Color.FromRgb(255, 255, 0);
        public static Color Red { get; } = Color.FromRgb(255, 0, 0);
    }
}
