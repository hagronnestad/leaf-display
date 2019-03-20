using LeafLib.Extensions;
using Newtonsoft.Json;

namespace LeafLib.Models {

    public class TimeRequiredToFull {

        [JsonProperty("HourRequiredToFull")]
        public string HourRequiredToFull { get; set; }

        [JsonProperty("MinutesRequiredToFull")]
        public string MinutesRequiredToFull { get; set; }

        [JsonIgnore]
        public long TotalMinutesToFull
        {
            get
            {
                var h = HourRequiredToFull.StringToInt();
                var m = MinutesRequiredToFull.StringToInt();

                return (h * 60) + m;
            }
        }
    }
}