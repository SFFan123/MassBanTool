using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using MassBanToolMP.ViewModels;
using TwitchLib.Api;
using MassBanToolMP.Models;

namespace MassBanToolMP
{
    internal class Program
    {
        public static Version Version { get; } = new(1,1,5,1);
        public static TwitchAPI API;
        public static SettingsWrapper settings;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                LogViewModel.Log("Try loading setting for this User.");
                var fileName = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), "MassBanTool", "MassBanToolData.json");

                if (File.Exists(fileName))
                    try
                    {
                        string filecontent = File.ReadAllText(fileName);
                        filecontent = filecontent.Trim();
                        settings = SettingsWrapper.FromJson(filecontent);
                    }
                    catch (Exception e)
                    {
                        LogViewModel.Log("Something went wrong loading setting for this User. - " + e.Message);
                    }
            }
            catch (Exception e)
            {
                try
                {
                    Console.WriteLine(e);
                    LogViewModel.Log(e.Message);
                }
                catch {}
            }

            API = new TwitchAPI();
            API.Settings.ClientId = "hbhswgdfb452bz3o63f8strz7u0jgp";

            if (OperatingSystem.IsLinux())
            {
                // Check if there is a GUI.
                if(string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")))
                {
                    throw new PlatformNotSupportedException("No GUI Session detected cannot start GUI: XDG_CURRENT_DESKTOP doesn't exists/has no value.");
                }
            }
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
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
