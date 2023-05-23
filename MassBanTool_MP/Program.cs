using System;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.ReactiveUI;
using MassBanToolMP.Models;
using MassBanToolMP.ViewModels;
using TwitchLib.Api;

[assembly: AssemblyVersion("1.1.5.6")]
namespace MassBanToolMP
{
    internal class Program
    {
        public static TwitchAPI API;
        public static SettingsWrapper settings;
        public static Version Version { get; } = Assembly.GetEntryAssembly().GetName().Version;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                LogViewModel.Log("Using Runtime: " + Environment.Version);
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
                catch
                {
                }
            }

            API = new TwitchAPI();
            API.Settings.ClientId = "hbhswgdfb452bz3o63f8strz7u0jgp";

            if (OperatingSystem.IsLinux())
            {
                // Check if there is a GUI.
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP")))
                {
                    Console.Error.WriteLine(
                        "No GUI Session detected cannot start GUI: XDG_CURRENT_DESKTOP doesn't exists/has no value.");
                    Environment.Exit(-2);
                }
            }

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
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