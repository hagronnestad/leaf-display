using Newtonsoft.Json;

namespace LeafLib.Models {

    public class InitialAppResult {

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("baseprm")]
        public string BasePrm { get; set; }
    }
}