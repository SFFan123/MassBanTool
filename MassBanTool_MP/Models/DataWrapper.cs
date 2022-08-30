using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MassBanToolMP.Models
{
    public class DataWrapper
    {
        public DataWrapper()
        {
            lastVisitedChannel = new HashSet<string>();
        }
        public HashSet<string> lastVisitedChannel { get; set; }

        [DefaultValue(null)]
        public HashSet<string>? AllowedActions { get; set; }

        public int message_delay { get; set; }

        public bool checkForUpdates { get; set; }

        public bool includePrereleases { get; set; }

        public bool LoadCredentialOnStartup { get; set; }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static DataWrapper? FromJson(string JSON)
        {
            return JsonConvert.DeserializeObject<DataWrapper>(JSON, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}