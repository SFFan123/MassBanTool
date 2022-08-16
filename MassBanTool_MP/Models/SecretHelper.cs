using System;
using System.Diagnostics;
using MassBanToolMP.ViewModels;

namespace MassBanToolMP.Models
{
    internal class SecretHelper
    {
        public static Tuple<string, string> GetCredentials()
        {
            LogViewModel.Log("Checking OS");
            if (OperatingSystem.IsWindows())
            {
                LogViewModel.Log("Trying to get Credentials from Window Credential Manager");
                return GetCredentialsOnWindows();
            }
            if (OperatingSystem.IsLinux())
            {
                LogViewModel.Log("!EXPERIMENTAL! Trying to get Credentials from Gnome keyring");
                return GetCredentialsOnLinux();
            }

            LogViewModel.Log("OS Credential manager unknown.");
            return new Tuple<string, string>("", "");
        }


        private static Tuple<string, string> GetCredentialsOnWindows()
        {
            using var cred = new CredentialManagement.Credential();
            cred.Target = "MassBanTool";
            cred.Load();
            if (cred.Exists())
            {
                return new Tuple<string, string>(cred.Username, cred.Password);
            }
            LogViewModel.Log("Unknown Exception while fetching credentials from Windows Credential Manager");
            throw new Exception("Unknown Exception while fetching credentials from Windows Credential Manager");
        }

        private static Tuple<string, string> GetCredentialsOnLinux()
        {
            //check for secret-tool
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = "-c \"command -v secret-tool\"", RedirectStandardOutput = true}; 
            Process proc = new Process() { StartInfo = startInfo };
            proc.Start();

            string? Line = proc.StandardOutput.ReadLine();
            if (Line == null || Line == "secret-tool could not be found")
            {
                throw new Exception("Secret-Tool is required for storing/retrieving credentials on Linux systems");
            }
            
            // use secret tool to get check for credentials for this tool


            // get credentials and return.
            


            return new Tuple<string, string>("", "");
        }
    }
}
