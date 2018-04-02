using Common;
using Common.Models;
using LeafLib;
using LeafLib.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace TestConsole {

    internal class Program {

        private static async Task Main(string[] args) {
            var tableConnectionString = args[2];
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

            Console.ReadLine();
        }

        public async Task<BatteryStatusRecordsRequestResult> LoginAndGetSoCAsync(string[] args) {
            var lc = new LeafClient(args[0], args[1]);

            BatteryStatusRecordsRequestResult bsr = null;

            try {
                Console.WriteLine("Logging in...");
                var loginResult = await lc.LogIn();

                Console.WriteLine("Requesting battery status check...");
                var requestBatteryStatusCheckResult = await lc.RequestBatteryStatusCheck();

                Console.WriteLine("Waiting for battery status check to finish...");
                var wait = await lc.WaitForBatteryStatusCheckResult(requestBatteryStatusCheckResult.ResultKey);

                Console.WriteLine("Getting battery status record...");
                bsr = await lc.GetBatteryStatusRecord();

                Console.WriteLine($"SoC Record: {bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent}%");
                Console.WriteLine($"Updated: {bsr?.BatteryStatusRecord?.NotificationDateAndTime}");

            } catch (Exception e) {
                Console.WriteLine($"Could not get data from NissanConnect!");
                Console.WriteLine($"Error: {e.Message}");
            }

            if (bsr == null) {
                Console.WriteLine($"Trying to get last battery status record as a fallback...");

                try {
                    bsr = await lc.GetBatteryStatusRecord();
                    Console.WriteLine($"SoC Record: {bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent}%");
                    Console.WriteLine($"Updated: {bsr?.BatteryStatusRecord?.NotificationDateAndTime}");

                } catch (Exception f) {
                    Console.WriteLine($"Error: {f.Message}");
                }
            }

            return bsr;
        }
    }
}