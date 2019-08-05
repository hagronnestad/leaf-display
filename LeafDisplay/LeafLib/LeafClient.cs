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
        private const string API_BASE_URI = "https://gdcportalgw.its-mo.com/api_v190426_NE/gdc/";
        private const int API_TIME_OUT = 30000;

        private HttpClient client = new HttpClient();

        private readonly string _email;
        private readonly string _encryptedPassword;
        private readonly string _regionCode;

        private string _customSessionId;
        private string _vin;

        private readonly string initialAppStrings = "9s5rfKVuMrT03RtzajWNcA";

        // Obtained from InitialApp_v2.php, might be static
        public string basePrm_blowfishEcbKey = "88dSp7wWnV3bvv9Z88zEwg";

        public LeafClient(string email, string encryptedPassword, string regionCode = RegionConstants.Europe) {
            client.BaseAddress = new Uri(API_BASE_URI);
            client.Timeout = TimeSpan.FromMilliseconds(API_TIME_OUT);

            _email = email;
            _encryptedPassword = encryptedPassword;
            _regionCode = regionCode;
        }

        public async Task<InitialAppResult> InitialApp_v2() {
            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("initial_app_str", initialAppStrings),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("InitialApp_v2.php", formContent);

            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<InitialAppResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not get InitialApp_v2 result.");

            return result;
        }

        public async Task<UserLoginRequestResult> LogIn() {
            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("UserId", _email),
                new KeyValuePair<string, string>("initial_app_str", initialAppStrings),
                new KeyValuePair<string, string>("RegionCode", _regionCode),
                new KeyValuePair<string, string>("Password", _encryptedPassword),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("UserLoginRequest.php", formContent);

            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UserLoginRequestResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not log in.");

            var vehicle = result.VehicleInfoList.VehicleInfoWithCustomSessionId.FirstOrDefault();
            _customSessionId = vehicle?.CustomSessionId ?? "";
            _vin = vehicle?.Vin ?? "";

            return result;
        }

        public async Task<BatteryStatusRecordsRequestResult> GetBatteryStatusRecord() {
            if (string.IsNullOrWhiteSpace(_customSessionId) || string.IsNullOrWhiteSpace(_vin)) {
                throw new Exception($"{nameof(_customSessionId)} and/or {nameof(_vin)} is not set. Log in first.");
            }

            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("custom_sessionid", _customSessionId),
                new KeyValuePair<string, string>("RegionCode", _regionCode),
                new KeyValuePair<string, string>("VIN", _vin),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("BatteryStatusRecordsRequest.php", formContent);

            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BatteryStatusRecordsRequestResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not get battery status record.");

            return result;
        }

        public async Task<BatteryStatusCheckRequestResult> RequestBatteryStatusCheck() {
            if (string.IsNullOrWhiteSpace(_customSessionId) || string.IsNullOrWhiteSpace(_vin)) {
                throw new Exception($"{nameof(_customSessionId)} and/or {nameof(_vin)} is not set. Log in first.");
            }

            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("UserId", _email),
                new KeyValuePair<string, string>("custom_sessionid", _customSessionId),
                new KeyValuePair<string, string>("RegionCode", _regionCode),
                new KeyValuePair<string, string>("VIN", _vin),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("BatteryStatusCheckRequest.php", formContent);

            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BatteryStatusCheckRequestResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not request battery status check.");

            return result;
        }

        public async Task<BatteryStatusCheckResult> GetBatteryStatusCheckResult(string resultKey) {
            if (string.IsNullOrWhiteSpace(resultKey)) throw new ArgumentNullException(nameof(resultKey));

            if (string.IsNullOrWhiteSpace(_customSessionId) || string.IsNullOrWhiteSpace(_vin)) {
                throw new Exception($"{nameof(_customSessionId)} and/or {nameof(_vin)} is not set. Log in first.");
            }

            var data = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("resultKey", resultKey),
                new KeyValuePair<string, string>("custom_sessionid", _customSessionId),
                new KeyValuePair<string, string>("RegionCode", _regionCode),
                new KeyValuePair<string, string>("VIN", _vin),
            };
            var formContent = new FormUrlEncodedContent(data);
            var response = await client.PostAsync("BatteryStatusCheckResultRequest.php", formContent);

            if (!response.IsSuccessStatusCode) {
                throw new Exception($"Request to '{response.RequestMessage.RequestUri}' failed with status code '{response.StatusCode}'.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<BatteryStatusCheckResult>(json);
            if (result == null || result.Status != 200) throw new Exception("Could not get battery status check result.");

            return result;
        }

        public async Task<BatteryStatusCheckResult> WaitForBatteryStatusCheckResult(string resultKey, int timeout = 60000) {
            var cancellationTokenSource = new CancellationTokenSource();

            var task = WaitForBatteryStatusCheckResultTask(resultKey, cancellationTokenSource.Token);

            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationTokenSource.Token)) == task) {
                return await task;

            } else {
                cancellationTokenSource.Cancel();
                throw new TimeoutException($"{nameof(WaitForBatteryStatusCheckResult)} timed out after {timeout} milliseconds.");
            }
        }

        private async Task<BatteryStatusCheckResult> WaitForBatteryStatusCheckResultTask(string resultKey, CancellationToken cancellationToken, int retryInterval = 2500) {
            BatteryStatusCheckResult result;

            while ((result = (await GetBatteryStatusCheckResult(resultKey)))?.ResponseFlag != "1" && !cancellationToken.IsCancellationRequested) {
                Thread.Sleep(retryInterval);
            }

            return result;
        }

    }
}