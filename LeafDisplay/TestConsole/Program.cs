using LeafLib;
using LeafLib.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleTest2 {

    internal class Program {

        private static async Task Main(string[] args) {

            //var r = await LoginAndGetSoCAsync(args);
            var r = await LoginAndOutputToJson(args[0], args[1], args[2]);
        }

        public static async Task<BatteryStatusRecordsRequestResult> LoginAndOutputToJson(string email, string password, string jsonPath) {
            var lc = new LeafClient(email, password);

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

            } catch (Exception e) {
                Console.WriteLine($"Could not get data from NissanConnect!");
                Console.WriteLine($"Error: {e.Message}");
            }

            if (bsr == null) {
                Console.WriteLine($"Trying to get last battery status record as a fallback...");

                try {
                    bsr = await lc.GetBatteryStatusRecord();

                } catch (Exception f) {
                    Console.WriteLine($"Error: {f.Message}");
                }
            }

            if (bsr == null) {
                Console.WriteLine("Could not get GetBatteryStatusRecord!");
                return null;
            }

            var jsonData = new {
                TimeStamp = bsr?.BatteryStatusRecord?.NotificationDateAndTime.ToString("dd\\/MM\\/yy HH\\:mm"),
                BatteryCapacity = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryCapacity,
                ChargingStatus = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryChargingStatus,
                SoC = bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent,
                PluginState = bsr?.BatteryStatusRecord?.PluginState,
                Range = bsr?.BatteryStatusRecord?.CruisingRangeAcOff,
                RangeAc = bsr?.BatteryStatusRecord?.CruisingRangeAcOn,
                ChargeTime = $"{bsr?.BatteryStatusRecord?.TimeRequiredToFull200?.HourRequiredToFull}:{bsr?.BatteryStatusRecord?.TimeRequiredToFull200?.MinutesRequiredToFull}",
            };

            var json = JsonConvert.SerializeObject(jsonData);

            Console.WriteLine(json);
            File.WriteAllText(jsonPath, json);

            return bsr;
        }

        public static async Task<BatteryStatusRecordsRequestResult> LoginAndGetSoCAsync(string[] args) {
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
                Console.WriteLine($"PluginState: {bsr?.BatteryStatusRecord?.PluginState}");
                Console.WriteLine($"CruisingRangeAcOff: {bsr?.BatteryStatusRecord?.CruisingRangeAcOff}");
                Console.WriteLine($"CruisingRangeAcOn: {bsr?.BatteryStatusRecord?.CruisingRangeAcOn}");
                Console.WriteLine($"TimeRequiredToFull: {bsr?.BatteryStatusRecord?.TimeRequiredToFull?.HourRequiredToFull}:{bsr?.BatteryStatusRecord.TimeRequiredToFull?.MinutesRequiredToFull}");
                Console.WriteLine($"TimeRequiredToFull200: {bsr?.BatteryStatusRecord?.TimeRequiredToFull200?.HourRequiredToFull}:{bsr?.BatteryStatusRecord.TimeRequiredToFull200?.MinutesRequiredToFull}");
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