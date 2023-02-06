using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace MassBanToolMP.Models
{
    public class SettingsWrapper
    {
        public SettingsWrapper()
        {
            LastVisitedChannels = new HashSet<string>();
        }

        public Version Version { get; set; }

        [JsonProperty("lastVisitedChannel")]
        public HashSet<string> LastVisitedChannels { get; set; }

        [DefaultValue(null)]
        public HashSet<string>? AllowedActions { get; set; }

        public int RequestsPerMinute { get; set; }

        [JsonProperty("checkForUpdates")]
        public bool CheckForUpdates { get; set; }

        [JsonProperty("includePrereleases")]
        public bool IncludePrereleases { get; set; }

        public bool LoadCredentialOnStartup { get; set; }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SettingsWrapper? FromJson(string JSON)
        {
            return JsonConvert.DeserializeObject<SettingsWrapper>(JSON, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, MissingMemberHandling = MissingMemberHandling.Ignore, });
        }
    }
}