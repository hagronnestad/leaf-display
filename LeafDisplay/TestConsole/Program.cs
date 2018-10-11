using LeafLib;
using LeafLib.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleTest2 {

    internal class Program {

        private static async Task Main(string[] args) {

            if (args.Length == 0 || args.Length < 2) {
                PrintHelp();
#if DEBUG
                Console.ReadLine();
#endif
                return;
            }

            var userName = args[0];
            var password = args[1];
            var getLast = false;
            var fileName = string.Empty;

            args = args.Skip(2).ToArray();

            for (int i = 0; i < args.Length; i++) {
                switch (args[i].Trim().ToLower()) {

                    case "-o":
                        if (args.Length >= i + 1) {
                            fileName = args[i + 1];
                            i++;
                        }
                        break;

                    case "-last":
                        getLast = true;
                        break;
                }
            }

            await ExecuteCommand(userName, password, getLast, fileName);

#if DEBUG
            Console.ReadLine();
#endif
        }

        public static async Task ExecuteCommand(string email, string password, bool getLast = false, string fileName = null) {
            var lc = new LeafClient(email, password);

            BatteryStatusRecordsRequestResult bsr = null;

            try {
                Console.WriteLine("Logging in...");
                var loginResult = await lc.LogIn();

                if (!getLast) {
                    Console.WriteLine("Requesting battery status check...");
                    var requestBatteryStatusCheckResult = await lc.RequestBatteryStatusCheck();

                    Console.WriteLine("Waiting for battery status check to finish...");
                    var wait = await lc.WaitForBatteryStatusCheckResult(requestBatteryStatusCheckResult.ResultKey);

                    Console.WriteLine("Getting battery status record...");
                    bsr = await lc.GetBatteryStatusRecord();
                }

            } catch (Exception e) {
                Console.WriteLine($"Could not get data from Nissan Connect!");
                Console.WriteLine($"Error: {e.Message}");
            }

            if (bsr == null) {
                try {
                    Console.WriteLine("Getting battery status record...");
                    bsr = await lc.GetBatteryStatusRecord();

                } catch (Exception e) {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }

            if (bsr == null) {
                Console.WriteLine("Could not get the last battery status record.");
                Console.WriteLine("Exiting...");
                return;
            }

            var jsonData = new {
                TimeStampUtc = bsr?.BatteryStatusRecord?.NotificationDateAndTime.ToString("dd\\/MM\\/yy HH\\:mm"),
                TimeStamp = bsr?.BatteryStatusRecord?.NotificationDateAndTimeAsLocal.ToString("dd\\/MM\\/yy HH\\:mm"),
                BatteryCapacity = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryCapacity,
                ChargingStatus = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryChargingStatus,
                SoC = bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent,
                PluginState = bsr?.BatteryStatusRecord?.PluginState,
                Range = bsr?.BatteryStatusRecord?.CruisingRangeAcOff,
                RangeAc = bsr?.BatteryStatusRecord?.CruisingRangeAcOn,
                ChargeTime = $"{bsr?.BatteryStatusRecord?.TimeRequiredToFull200?.HourRequiredToFull}:{bsr?.BatteryStatusRecord?.TimeRequiredToFull200?.MinutesRequiredToFull}",
            };

            var json = JsonConvert.SerializeObject(jsonData, Formatting.Indented);

            Console.WriteLine("\nRetrieved data:\n");
            Console.WriteLine(json);

            if (!string.IsNullOrWhiteSpace(fileName)) {
                try {
                    Console.WriteLine($"\nOutputting JSON data to '{fileName}'.");
                    File.WriteAllText(fileName, json);

                } catch (Exception e) {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        public static void PrintHelp() {
            Console.WriteLine("Usage: leafclient username password [-o {filename}] [-last]");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("\tusername\tYour Nissan Connect username.");
            Console.WriteLine("\tpassword\tYour blowfish encrypted password.");
            Console.WriteLine("\t\t\tEncrypt your password at http://sladex.org/blowfish.js/.");
            Console.WriteLine("\t\t\tKey: 'uyI5Dj9g8VCOFDnBRUbr3g'. Cipher mode: ECB. Output type: BASE64.");
            Console.WriteLine("");
            Console.WriteLine("\t-o\t\tOutputs the result as JSON to {filename}.");
            Console.WriteLine("\t-last\t\tDon't query live data from car.");
        }
    }
}