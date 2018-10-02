using Newtonsoft.Json;

namespace LeafLib.Models {

    public class BatteryStatusCheckRequestResult {

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("vin")]
        public string Vin { get; set; }

        [JsonProperty("resultKey")]
        public string ResultKey { get; set; }
    }
}