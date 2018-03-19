using LeafLib;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestConsole {

    internal class Program {

        private static async Task Main(string[] args) {

            var lc = new LeafClient(args[0], args[1]);

            try {

                Console.WriteLine("Logging in...");
                var loginResult = await lc.LogIn();

                Console.WriteLine("Requesting battery status check...");
                var requestBatteryStatusCheckResult = await lc.RequestBatteryStatusCheck();

                Console.WriteLine("Waiting for battery status check to finish...");
                var wait = await lc.WaitForBatteryStatusCheckResult(requestBatteryStatusCheckResult.ResultKey);

                Console.WriteLine("Getting battery status record...");
                var getBatteryStatusRecordResult = await lc.GetBatteryStatusRecord();

                Console.WriteLine($"SoC Record: {getBatteryStatusRecordResult?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent}%");
                Console.WriteLine($"Updated: {getBatteryStatusRecordResult?.BatteryStatusRecord?.NotificationDateAndTime}");

            } catch (Exception e) {
                Console.WriteLine($"Could not get data from NissanConnect!");
                Console.WriteLine($"Error: {e.Message}");

                Console.WriteLine($"Trying to get last battery status record as a fallback...");

                try {
                    var bsr = await lc.GetBatteryStatusRecord();
                    Console.WriteLine($"SoC Record: {bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent}%");
                    Console.WriteLine($"Updated: {bsr?.BatteryStatusRecord?.NotificationDateAndTime}");

                } catch (Exception f) {
                    Console.WriteLine($"Error: {f.Message}");
                }
            }

            Console.ReadLine();
        }
    }
}