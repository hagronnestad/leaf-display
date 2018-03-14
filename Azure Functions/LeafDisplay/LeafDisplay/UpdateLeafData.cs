using LeafLib;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Threading.Tasks;

namespace LeafDisplay {

    public static class UpdateLeafData {

        [FunctionName("UpdateLeafData")]
        public static async Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log) {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var email = Environment.GetEnvironmentVariable("email");
            var password = Environment.GetEnvironmentVariable("password");

            var lc = new LeafClient(email, password);
            var res = await lc.LogIn();
        }
    }
}