using Newtonsoft.Json;

namespace LeafLib.Models {

    public class VehicleInfo {

        [JsonProperty("nickname")]
        public string NickName { get; set; }

        [JsonProperty("telematicsEnabled")]
        public bool TelematicsEnabled { get; set; }

        [JsonProperty("vin")]
        public string Vin { get; set; }

        [JsonProperty("custom_sessionid")]
        public string CustomSessionId { get; set; }
    }
}