using LeafLib.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LeafLib.Extensions;

namespace LeafClient {

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
            var url = string.Empty;

            args = args.Skip(2).ToArray();

            for (int i = 0; i < args.Length; i++) {
                switch (args[i].Trim().ToLower()) {

                    case "-o":
                        if (args.Length >= i + 1) {
                            fileName = args[i + 1];
                            i++;
                        }
                        break;

                    case "-p":
                        if (args.Length >= i + 1) {
                            url = args[i + 1];
                            i++;
                        }
                        break;

                    case "-last":
                        getLast = true;
                        break;
                }
            }

            await ExecuteCommand(userName, password, getLast, fileName, url);

#if DEBUG
            Console.ReadLine();
#endif
        }

        public static async Task ExecuteCommand(string email, string password, bool getLast = false, string fileName = null, string url = null) {
            var lc = new LeafLib.LeafClient(email, password);

            BatteryStatusRecordsRequestResult bsr = null;
            UserLoginRequestResult loginResult = null;

            try {
                Console.WriteLine("Getting InitialApp_v2 result...");
                var iar = await lc.InitialApp_v2();
                if (iar.BasePrm != lc.basePrm_blowfishEcbKey) {
                    Console.WriteLine($"BasePrm from InitialApp_v2 has changed, which means the password hash has to be updated!");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Logging in...");
                loginResult = await lc.LogIn();

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

            var json = JsonConvert.SerializeObject(new NissanLeafStatusDto {

                BatteryCapacity = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryCapacity?.StringToInt() ?? 0,
                BatteryRemainingAmount = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryRemainingAmount.StringToInt() ?? 0,
                BatteryRemainingAmountkWH = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryRemainingAmountkWH.StringToInt() ?? 0,
                BatteryRemainingAmountWH = bsr?.BatteryStatusRecord?.BatteryStatus?.BatteryRemainingAmountWH.StringToInt() ?? 0,
                CruisingRangeAcOff = bsr?.BatteryStatusRecord?.CruisingRangeAcOff.StringToInt() ?? 0,
                CruisingRangeAcOn = bsr?.BatteryStatusRecord?.CruisingRangeAcOn.StringToInt() ?? 0,
                MinutesToFull = bsr?.BatteryStatusRecord?.TimeRequiredToFull?.TotalMinutesToFull ?? 0,
                MinutesToFull200 = bsr?.BatteryStatusRecord?.TimeRequiredToFull200?.TotalMinutesToFull ?? 0,
                MinutesToFull200_6kW = bsr?.BatteryStatusRecord?.TimeRequiredToFull200_6kW?.TotalMinutesToFull ?? 0,
                NickName = loginResult?.VehicleInfoList.VehicleInfoWithCustomSessionId.FirstOrDefault().NickName,
                Vin = loginResult?.VehicleInfoList.VehicleInfoWithCustomSessionId.FirstOrDefault().Vin,
                PluginState = bsr?.BatteryStatusRecord?.PluginState,
                ChargingStatus = bsr.BatteryStatusRecord?.BatteryStatus?.BatteryChargingStatus,
                StateOfCharge = bsr?.BatteryStatusRecord?.BatteryStatus?.StateOfCharge?.Percent?.StringToInt() ?? 0,
                Timestamp = bsr?.BatteryStatusRecord?.NotificationDateAndTime ?? DateTime.MinValue

            }, Formatting.Indented);

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

            if (!string.IsNullOrWhiteSpace(url)) {
                try {
                    Console.WriteLine($"\nPosting JSON data to '{url}'.");

                    var client = new HttpClient();

                    var result = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                } catch (Exception e) {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        public static void PrintHelp() {
            Console.WriteLine("Usage: leafclient username password [-o {filename}] [-p {url}] [-last]");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("\tusername\tYour Nissan Connect username.");
            Console.WriteLine("\tpassword\tYour blowfish encrypted password.");
            Console.WriteLine("\t\t\tEncrypt your password at http://sladex.org/blowfish.js/.");
            Console.WriteLine("\t\t\tKey: 'uyI5Dj9g8VCOFDnBRUbr3g'. Cipher mode: ECB. Output type: BASE64.");
            Console.WriteLine("");
            Console.WriteLine("\t-o\t\tOutputs the result as JSON to {filename}.");
            Console.WriteLine("\t-p\t\tPosts the result as JSON to {url}.");
            Console.WriteLine("\t-last\t\tDon't query live data from car.");
        }
    }
}