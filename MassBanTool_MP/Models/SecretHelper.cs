using System;
using System.Diagnostics;
using CredentialManagement;
using MassBanToolMP.ViewModels;

namespace MassBanToolMP.Models
{
    internal class SecretHelper
    {
        public static string? GetCredentials()
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
            return null;
        }
        public static void StoreCredentials(string oauth)
        {
            if (OperatingSystem.IsWindows())
            {
                StoreCredentialsWindows(ref oauth);
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                StoreCredentialsLinux(ref oauth);
                return;
            }

            LogViewModel.Log("ERROR, Unknown OS can not save credentials.");
        }
        private static string? GetCredentialsOnWindows()
        {
            try
            {
                using var cred = new CredentialManagement.Credential();
                cred.Target = "MassBanTool";
                cred.Load();
                if (cred.Exists())
                {
                    return cred.Password;
                }
                LogViewModel.Log("Credentials not found. using Windows Credential Manager");
                return null;
            }
            catch (Exception e)
            {
                LogViewModel.Log("Unexpected Exception while fetching credentials from Windows Credential Manager\n\n"+e.Message);
                throw new Exception("Unexpected Exception while fetching credentials from Windows Credential Manager");
            }
        }
        private static string? GetCredentialsOnLinux()
        {
            //check for secret-tool
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = "-c \"command -v secret-tool\"", RedirectStandardOutput = true}; 
            Process proc = new Process() { StartInfo = startInfo };
            proc.Start();
            proc.WaitForExit(5000);
            string? Line = proc.StandardOutput.ReadLine();
            if (Line == null || Line == "secret-tool could not be found")
            {
                throw new Exception("Secret-Tool is required for storing/retrieving credentials on Linux systems");
            }
            proc.Kill();
            proc.Dispose();


            //TODO catch if there are more than 1 credentials.
            // use secret tool to get check for credentials for this tool
            startInfo = new ProcessStartInfo()
            {
                FileName = "secret-tool", 
                Arguments = "search --all --unlock App MassBanTool",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            proc = new Process() { StartInfo = startInfo };
            
            
            proc.Start();
            proc.WaitForExit(5000);
            Line = proc.StandardOutput.ReadToEnd();
            string Line2 = proc.StandardError.ReadToEnd();

            if (string.IsNullOrEmpty(Line))
            {
                LogViewModel.Log("Credentials not found. using secret-tool", "GetCredentialsOnLinux");
                return null;
            }

            // get credentials and return.
            string[] output = Line.Split(Environment.NewLine);
            string[] erOutput = Line2.Split(Environment.NewLine);
            
            string oauth = string.Empty;
            foreach (string outline in output)
            {
                string[] tuple = outline.Split(" = ");
                switch (tuple[0])
                {
                    case "secret":
                        oauth = tuple[1];
                        continue;

                    default:
                        continue;
                }
            }
            return oauth;
        }
        private static void StoreCredentialsLinux(ref string oauth)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "bash", 
                Arguments = $"-c \"secret-tool store --label=\'MassBanTool\' App MassBanTool\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
            };
            var proc = new Process() { StartInfo = startInfo };
            proc.Start();
            proc.StandardInput.Write(oauth);
            proc.StandardInput.Flush();
            proc.StandardInput.Close();
            
            proc.WaitForExit();
            
            // secret-tool store --label='MassBanTool' App MassBanTool
            // write to stdin oauth EOF
        }
        private static void StoreCredentialsWindows(ref string oauth)
        {
            using (var cred = new Credential())
            {
                cred.Password = oauth;
                cred.Target = "MassBanTool";
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Save();
            }
        }
    }
}
