using Newtonsoft.Json;

namespace LeafLib.Models {

    public class BatteryStatus {

        [JsonProperty("BatteryChargingStatus")]
        public string BatteryChargingStatus { get; set; } // NOT_CHARGING, NORMAL_CHARGING

        [JsonProperty("BatteryCapacity")]
        public string BatteryCapacity { get; set; }

        [JsonProperty("BatteryRemainingAmount")]
        public string BatteryRemainingAmount { get; set; }

        [JsonProperty("BatteryRemainingAmountWH")]
        public string BatteryRemainingAmountWH { get; set; }

        [JsonProperty("BatteryRemainingAmountkWH")]
        public string BatteryRemainingAmountkWH { get; set; }

        [JsonProperty("SOC")]
        public StateOfCharge StateOfCharge { get; set; }
    }
}