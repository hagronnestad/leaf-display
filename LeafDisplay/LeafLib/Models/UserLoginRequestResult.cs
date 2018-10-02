using Newtonsoft.Json;

namespace LeafLib.Models {

    public class UserLoginRequestResult {

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("sessionId")]
        public string SessionId { get; set; }

        [JsonProperty("VehicleInfoList")]
        public VehicleInfoList VehicleInfoList { get; set; }
    }
}