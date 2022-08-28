using System;
using System.IO;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using ReactiveUI;

namespace MassBanToolMP.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        public static LogViewModel Instance { get; private set; }
        private static Regex checkForTimestamp;
        private string _logLines;

        public string logLines
        {
            get => _logLines;
            set => SetProperty(ref _logLines, value);
        }

        public ReactiveCommand<Window, Unit> SaveLogCommand { get; }

        public LogViewModel()
        {
            Instance = this;
            logLines = string.Empty;
            checkForTimestamp = new Regex(@"^\d{1,2}\.\d{1,2}\.\d{4}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(5));
            SaveLogCommand = ReactiveCommand.Create<Window>(SaveLog);
        }

        private async void SaveLog(Window window)
        {
            var filediag = new SaveFileDialog();
            filediag.InitialFileName = $"MassBanTool_{DateTime.Now.ToLongDateString()}.log";
            filediag.DefaultExtension = ".log";
            filediag.Title = "Save Log as";
            
            string? filename = await filediag.ShowAsync(window);

            if (filename == null)
                return;

            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Log("Error while saving log, Directory not found.");
            }
            
            try
            {
                await File.WriteAllTextAsync(filename, logLines, Encoding.Default);
            }
            catch (Exception e)
            {
                Log("Error while saving Logfile: " + e.Message);
            }
        }
        
        public static void Log(string message, [CallerMemberName]string source = null)
        {
            if (string.IsNullOrEmpty(message))
                return;

            StringBuilder sb = new StringBuilder();

            if (!checkForTimestamp.IsMatch(message))
            {
                sb.Append(DateTime.Now.ToLongTimeString());
                sb.Append(": ");
            }
            if(source != null)
            {
                sb.Append(source);
            }

            sb.Append("#: ");

            sb.Append(message);

            if (!string.IsNullOrEmpty(Instance.logLines))
            {
                Instance.logLines += Environment.NewLine;
            }

            Instance.logLines += sb;
            Instance.RaisePropertyChanged(nameof(logLines));
        }
    }
}
