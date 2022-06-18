using System;
using System.Diagnostics;

namespace MassBanToolMP.Models
{
    internal class SecretHelper
    {
        public static Tuple<string, string> GetCredentials()
        {
            if (OperatingSystem.IsWindows())
            {
                return GetCredentialsOnWindows();
            }
            if (OperatingSystem.IsLinux())
            {
                return GetCredentialsOnLinux();
            }



            return new Tuple<string, string>("", "");

            //TODO
            throw new NotImplementedException();
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
