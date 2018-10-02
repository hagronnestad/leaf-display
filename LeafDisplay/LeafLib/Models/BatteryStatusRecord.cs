using Newtonsoft.Json;
using System;

namespace LeafLib.Models {

    public class BatteryStatusRecord {

        [JsonProperty("OperationResult")]
        public string OperationResult { get; set; }

        [JsonProperty("OperationDateAndTime")]
        public DateTime OperationDateAndTime { get; set; }

        [JsonProperty("PluginState")]
        public string PluginState { get; set; } //NOT_CONNECTED, CONNECTED

        [JsonProperty("CruisingRangeAcOn")]
        public string CruisingRangeAcOn { get; set; }

        [JsonProperty("CruisingRangeAcOff")]
        public string CruisingRangeAcOff { get; set; }

        [JsonProperty("TimeRequiredToFull")]
        public TimeRequiredToFull TimeRequiredToFull { get; set; }

        [JsonProperty("TimeRequiredToFull200")]
        public TimeRequiredToFull TimeRequiredToFull200 { get; set; }

        [JsonProperty("TimeRequiredToFull200_6kW")]
        public TimeRequiredToFull TimeRequiredToFull200_6kW { get; set; }

        [JsonProperty("NotificationDateAndTime")]
        public DateTime NotificationDateAndTime { get; set; }

        [JsonProperty("TargetDate")]
        public DateTime TargetDate { get; set; }

        [JsonProperty("BatteryStatus")]
        public BatteryStatus BatteryStatus { get; set; }
    }
}