using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Common.Models {

    public class LeafDataEntity : TableEntity {

        // https://docs.microsoft.com/en-us/azure/cosmos-db/table-storage-how-to-use-dotnet#add-an-entity-to-a-table

        // From docs: Also, your entity type must expose a parameter-less constructor.
        public LeafDataEntity() {
            this.PartitionKey = nameof(LeafDataEntity);
            this.RowKey = nameof(LeafDataEntity);
        }

        public DateTime DateTime { get; set; }
        public int BatteryLevelPercent { get; set; }
    }
}