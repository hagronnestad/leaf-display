using Newtonsoft.Json;

namespace LeafLib.Models {

    public class TimeRequiredToFull {

        [JsonProperty("HourRequiredToFull")]
        public string HourRequiredToFull { get; set; }

        [JsonProperty("MinutesRequiredToFull")]
        public string MinutesRequiredToFull { get; set; }
    }
}