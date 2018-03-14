using LeafLib;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TestConsole {

    internal class Program {

        private static async Task Main(string[] args) {

            var lc = new LeafClient(args[0], args[1]);

            try {

                Debug.WriteLine("Logging in...");
                var loginResult = await lc.LogIn();

                Debug.WriteLine("Requesting battery status check...");
                var requestBatteryStatusCheckResult = await lc.RequestBatteryStatusCheck();

                Debug.WriteLine("Waiting for battery status check to finish...");
                var wait = await lc.WaitForBatteryStatusCheckResult(requestBatteryStatusCheckResult.ResultKey);

                Debug.WriteLine("Getting battery status record...");
                var getBatteryStatusRecordResult = await lc.GetBatteryStatusRecord();

                Debug.WriteLine($"SoC Record: {getBatteryStatusRecordResult?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent}%");
                Debug.WriteLine($"Updated: {getBatteryStatusRecordResult?.BatteryStatusRecord?.NotificationDateAndTime}");

            } catch (Exception e) {
                Debug.WriteLine($"Could not get data from NissanConnect!");
                Debug.WriteLine($"Error: {e.Message}");

                Debug.WriteLine($"Trying to get last battery status record as a fallback...");

                try {
                    var bsr = await lc.GetBatteryStatusRecord();
                    Debug.WriteLine($"SoC Record: {bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent}%");
                    Debug.WriteLine($"Updated: {bsr?.BatteryStatusRecord?.NotificationDateAndTime}");

                } catch (Exception f) {
                    Debug.WriteLine($"Error: {f.Message}");
                }
            }

        }
    }
}