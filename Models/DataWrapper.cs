using System.Collections.Generic;
using Newtonsoft.Json;

namespace MassBanTool
{
    public class DataWrapper
    {
        public HashSet<string> lastVisitedChannel { get; set; }

        public HashSet<string> allowedActions { get; set; }

        public int message_delay { get; set; }


        public string toJSON()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static DataWrapper fromJson(string JSON)
        {
            return JsonConvert.DeserializeObject<DataWrapper>(JSON,
                new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
        }
    }
}