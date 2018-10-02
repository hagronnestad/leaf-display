using Newtonsoft.Json;

namespace LeafLib.Models {

    public class BatteryStatusRecordsRequestResult {

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("BatteryStatusRecords")]
        public BatteryStatusRecord BatteryStatusRecord { get; set; }
    }
}