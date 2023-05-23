using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MassBanToolMP.Helper
{
    public class HttpHelper
    {
        public static async Task<HttpResponseMessage> FetchPlainPage(Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Headers =
                    {
                        { HttpRequestHeader.Accept.ToString(), "text/plain" },
                        { "user-agent", "MassBanTool/" + Program.Version }
                    }
                };

                return await client.SendAsync(httpRequestMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Stable Release, Pre Release</returns>
        /// <exception cref="HttpRequestException"></exception>
        public static async Task<Tuple<JObject, JObject>> FetchGitHubReleases()
        {
            Uri uri = new Uri("https://api.github.com/repos/SFFan123/MassBanTool/releases");

            HttpResponseMessage response;
            JObject pre;
            JObject stable;

            using (HttpClient client = new ())
            {
                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = uri,
                    Headers =
                    {
                        { HttpRequestHeader.Accept.ToString(), "application/json" },
                        { "user-agent", "MassBanTool/" + Program.Version }
                    }
                };

                response = await client.SendAsync(httpRequestMessage);
            }

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                JArray jArray = JArray.Parse(result);

                List<JObject> releases = new List<JObject>();
                foreach (var item in jArray)
                {
                    releases.Add(JObject.Parse(item.ToString()));
                }
                
                stable = releases.Where(x => !(bool)x["prerelease"]).FirstOrDefault(x => !(bool)x["draft"]);
    
                pre = releases.FirstOrDefault(x => !(bool)x["draft"]);
                
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
            
            return new Tuple<JObject, JObject>(stable, pre);
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
