using LeafLib.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LeafLib {

    public class LeafClient {
        private const string API_BASE_URI = "https://gdcportalgw.its-mo.com/gworchest_160803EC/gdc/";
        private const int API_TIME_OUT = 30000;

        private HttpClient client = new HttpClient();

        private readonly string email;
        private readonly string password;
        private readonly string regionCode;

        private string customSessionId;
        private string vin;

        private readonly string initialAppStrings = "geORNtsZe5I4lRGjG9GZiA";

        // Obtained from InitialApp.php, but seems to be static
        private readonly string blowfishEcbKey = "uyI5Dj9g8VCOFDnBRUbr3g";

        public LeafClient(string email, string password, string regionCode = RegionConstants.Europe) {
            client.BaseAddress = new Uri(API_BASE_URI);
            client.Timeout = TimeSpan.FromMilliseconds(API_TIME_OUT);

            this.email = email;
            this.password = password;
            this.regionCode = regionCode;
        }

        public async Task<UserLoginRequestResult> LogIn() {
            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("UserId", email),
                new KeyValuePair<string, string>("initial_app_strings", initialAppStrings),
                new KeyValuePair<string, string>("RegionCode", regionCode),
                new KeyValuePair<string, string>("Password", password),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("UserLoginRequest.php", formContent);

            if (!response.IsSuccessStatusCode) throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<UserLoginRequestResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not log in.");

            var vehicle = result.VehicleInfoList.VehicleInfoWithCustomSessionId.FirstOrDefault();
            customSessionId = vehicle?.CustomSessionId ?? "";
            vin = vehicle?.Vin ?? "";

            return result;
        }

        public async Task<BatteryStatusRecordsRequestResult> GetBatteryStatusRecord() {
            if (string.IsNullOrWhiteSpace(customSessionId) || string.IsNullOrWhiteSpace(vin)) {
                throw new Exception($"{nameof(customSessionId)} and/or {nameof(vin)} is not set. Log in first.");
            }

            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("custom_sessionid", customSessionId),
                new KeyValuePair<string, string>("RegionCode", regionCode),
                new KeyValuePair<string, string>("VIN", vin),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("BatteryStatusRecordsRequest.php", formContent);

            if (!response.IsSuccessStatusCode) throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<BatteryStatusRecordsRequestResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not get battery status record.");

            return result;
        }

        public async Task<BatteryStatusCheckRequestResult> RequestBatteryStatusCheck() {
            if (string.IsNullOrWhiteSpace(customSessionId) || string.IsNullOrWhiteSpace(vin)) {
                throw new Exception($"{nameof(customSessionId)} and/or {nameof(vin)} is not set. Log in first.");
            }

            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("UserId", email),
                new KeyValuePair<string, string>("custom_sessionid", customSessionId),
                new KeyValuePair<string, string>("RegionCode", regionCode),
                new KeyValuePair<string, string>("VIN", vin),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("BatteryStatusCheckRequest.php", formContent);

            if (!response.IsSuccessStatusCode) throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<BatteryStatusCheckRequestResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not request battery status check.");

            return result;
        }

        public async Task<BatteryStatusCheckResult> GetBatteryStatusCheckResult(string resultKey) {
            if (string.IsNullOrWhiteSpace(resultKey)) throw new ArgumentNullException(nameof(resultKey));

            if (string.IsNullOrWhiteSpace(customSessionId) || string.IsNullOrWhiteSpace(vin)) {
                throw new Exception($"{nameof(customSessionId)} and/or {nameof(vin)} is not set. Log in first.");
            }

            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("resultKey", resultKey),
                new KeyValuePair<string, string>("custom_sessionid", customSessionId),
                new KeyValuePair<string, string>("RegionCode", regionCode),
                new KeyValuePair<string, string>("VIN", vin),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("BatteryStatusCheckResultRequest.php", formContent);

            if (!response.IsSuccessStatusCode) throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<BatteryStatusCheckResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could get battery status check result.");

            return result;
        }

        public async Task<BatteryStatusCheckResult> WaitForBatteryStatusCheckResult(string resultKey, int timeout = 60000) {
            var cancellationTokenSource = new CancellationTokenSource();

            var task = WaitForBatteryStatusCheckResultTaskAsync(resultKey, cancellationTokenSource.Token);

            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token)) == task) {
                return await task;

            } else {
                cancellationTokenSource.Cancel();
                throw new TimeoutException($"{nameof(WaitForBatteryStatusCheckResult)} timed out after {timeout} milliseconds.");
            }
        }

        private async Task<BatteryStatusCheckResult> WaitForBatteryStatusCheckResultTaskAsync(string resultKey, CancellationToken cancellationToken, int retryInterval = 2500) {
            BatteryStatusCheckResult result;

            while ((result = (await GetBatteryStatusCheckResult(resultKey)))?.ResponseFlag != "1" && !cancellationToken.IsCancellationRequested) {
                Thread.Sleep(retryInterval);
            }

            return result;
        }

    }
}