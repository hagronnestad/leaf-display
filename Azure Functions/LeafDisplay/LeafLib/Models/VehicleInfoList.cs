using Newtonsoft.Json;
using System.Collections.Generic;

namespace LeafLib.Models {

    public class VehicleInfoList {

        [JsonProperty("VehicleInfo")]
        public List<VehicleInfo> VehicleInfo { get; set; }

        [JsonProperty("vehicleInfo")]
        public List<VehicleInfo> VehicleInfoWithCustomSessionId { get; set; }
    }
}