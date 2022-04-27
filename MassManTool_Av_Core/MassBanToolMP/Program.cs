using Avalonia;
using Avalonia.ReactiveUI;
using System;
using CredentialManagement;

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
        
        public static Tuple<string, string> GetCredentials()
        {
            if (OperatingSystem.IsWindows())
            {
                using (var cred = new Credential())
                {
                    cred.Target = "MassBanTool";
                    cred.Load();
                    if (cred.Exists())
                    {
                        return new Tuple<string, string>(cred.Username, cred.Password);
                    }
                }
            }


            return new Tuple<string, string>("", "");

            //TODO
            throw new NotImplementedException();
        }
    }
}
