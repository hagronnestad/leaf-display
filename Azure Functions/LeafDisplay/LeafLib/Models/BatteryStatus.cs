using Newtonsoft.Json;
using System;

namespace LeafLib.Models {

    public class BatteryStatus {

        [JsonProperty("BatteryChargingStatus")]
        public String BatteryChargingStatus { get; set; }

        [JsonProperty("BatteryCapacity")]
        public String BatteryCapacity { get; set; }

        [JsonProperty("BatteryRemainingAmount")]
        public String BatteryRemainingAmount { get; set; }

        [JsonProperty("BatteryRemainingAmountWH")]
        public String BatteryRemainingAmountWH { get; set; }

        [JsonProperty("BatteryRemainingAmountkWH")]
        public String BatteryRemainingAmountkWH { get; set; }

        [JsonProperty("SOC")]
        public StateOfCharge StateOfCharge { get; set; }
    }
}