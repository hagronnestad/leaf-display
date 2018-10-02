using Newtonsoft.Json;
using System;

namespace LeafLib.Models {

    public class BatteryStatusCheckResult {

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("responseFlag")]
        public string ResponseFlag { get; set; }

        [JsonProperty("operationResult")]
        public string OperationResult { get; set; }

        [JsonProperty("timeStamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("cruisingRangeAcOn")]
        public string CruisingRangeAcOn { get; set; }

        [JsonProperty("cruisingRangeAcOff")]
        public string CruisingRangeAcOff { get; set; }

        [JsonProperty("currentChargeLevel")]
        public string CurrentChargeLevel { get; set; }

        [JsonProperty("chargeMode")]
        public string ChargeMode { get; set; }

        [JsonProperty("pluginState")]
        public string PluginState { get; set; }

        [JsonProperty("charging")]
        public string Charging { get; set; }

        [JsonProperty("chargeStatus")]
        public string ChargeStatus { get; set; }

        [JsonProperty("batteryDegradation")]
        public string BatteryDegradation { get; set; }

        [JsonProperty("batteryCapacity")]
        public string BatteryCapacity { get; set; }

        [JsonProperty("timeRequiredToFull")]
        public TimeRequiredToFull TimeRequiredToFull { get; set; }

        [JsonProperty("timeRequiredToFull200")]
        public TimeRequiredToFull TimeRequiredToFull200 { get; set; }

        [JsonProperty("TimeRequiredToFull200_6kW")]
        public TimeRequiredToFull TimeRequiredToFull200_6kW { get; set; }
    }
}