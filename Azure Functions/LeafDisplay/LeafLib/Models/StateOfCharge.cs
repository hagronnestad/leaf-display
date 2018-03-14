using Newtonsoft.Json;

namespace LeafLib.Models {

    public class StateOfCharge {

        [JsonProperty("Value")]
        public string Percent { get; set; }
    }
}