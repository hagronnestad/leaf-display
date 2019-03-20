using System;

namespace LeafLib.Models {

    public class NissanLeafStatusDto {

        public DateTime Timestamp { get; set; }

        public string NickName { get; set; }
        public string Vin { get; set; }

        public string PluginState { get; set; }

        public int CruisingRangeAcOn { get; set; }
        public int CruisingRangeAcOff { get; set; }

        public long MinutesToFull { get; set; }
        public long MinutesToFull200 { get; set; }
        public long MinutesToFull200_6kW { get; set; }

        public int BatteryCapacity { get; set; }

        public int BatteryRemainingAmount { get; set; }
        public int BatteryRemainingAmountWH { get; set; }
        public int BatteryRemainingAmountkWH { get; set; }

        public int StateOfCharge { get; set; }

    }
}