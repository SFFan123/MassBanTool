using System;
using System.Diagnostics;
using CredentialManagement;
using MassBanToolMP.ViewModels;

namespace MassBanToolMP.Models
{
    internal class SecretHelper
    {
        public static Tuple<string, string>? GetCredentials()
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
        public static void StoreCredentials(string username, string oauth)
        {
            if (OperatingSystem.IsWindows())
            {
                StoreCredentialsWindows(ref username, ref oauth);
                return;
            }

            if (OperatingSystem.IsLinux())
            {
                StoreCredentialsLinux(ref username, ref oauth);
                return;
            }

            LogViewModel.Log("ERROR, Unknown OS can not save credentials.");
        }
        private static Tuple<string, string>? GetCredentialsOnWindows()
        {
            try
            {
                using var cred = new CredentialManagement.Credential();
                cred.Target = "MassBanTool";
                cred.Load();
                if (cred.Exists())
                {
                    return new Tuple<string, string>(cred.Username, cred.Password);
                }
                LogViewModel.Log("Credentials not found. using Windows Credential Manager", "GetCredentialsOnWindows");
                return null;
            }
            catch (Exception e)
            {
                LogViewModel.Log("Unexpected Exception while fetching credentials from Windows Credential Manager\n\n"+e.Message);
                throw new Exception("Unexpected Exception while fetching credentials from Windows Credential Manager");
            }
        }
        private static Tuple<string, string>? GetCredentialsOnLinux()
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
            string user = string.Empty;
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
            foreach (string outline in erOutput)
            {
                string[] tuple = outline.Split(" = ");
                switch (tuple[0])
                {
                    case "attribute.User":
                        user = tuple[1];
                        continue;

                    default:
                        continue;
                }
            }

            return new Tuple<string, string>(user, oauth);
        }
        private static void StoreCredentialsLinux(ref string username, ref string oauth)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "bash", 
                Arguments = $"-c \"secret-tool store --label=\'MassBanTool\' App MassBanTool User {username}\"",
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
            
            // secret-tool store --label='MassBanTool' App MassBanTool User {username}
            // write to stdin oauth EOF


        }
        private static void StoreCredentialsWindows(ref string username, ref string oauth)
        {
            using (var cred = new Credential())
            {
                cred.Password = oauth;
                cred.Target = "MassBanTool";
                cred.Username = username;
                cred.Type = CredentialType.Generic;
                cred.PersistanceType = PersistanceType.LocalComputer;
                cred.Save();
            }
        }
    }
}
