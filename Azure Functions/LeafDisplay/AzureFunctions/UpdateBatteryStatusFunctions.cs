using Common;
using Common.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureFunctions {

    public static class UpdateBatteryStatusFunctions {

        [FunctionName(nameof(UpdateBatteryStatus))]
        public static void UpdateBatteryStatus([TimerTrigger("*/5 * * * * *")]TimerInfo myTimer, TraceWriter log) {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var email = Environment.GetEnvironmentVariable("email");
            var password = Environment.GetEnvironmentVariable("password");

            var tableConnectionString = Environment.GetEnvironmentVariable("table_connection_string");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(tableConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Globals.LEAFDISPLAY_TABLE_NAME);
            table.CreateIfNotExists();

            var entity = new LeafDataEntity() {
                DateTime = DateTime.UtcNow,
                BatteryLevelPercent = DateTime.UtcNow.Second
            };

            TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
            table.Execute(insertOperation);
        }
    }
}