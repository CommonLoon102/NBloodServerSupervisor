using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public class TorCheckService : ITorCheckService
    {
        private const string torExitNodesListUrl = "https://check.torproject.org/exit-addresses";

        private static DateTime lastListUpdate = DateTime.MinValue;
        private static DateTime lastUpdateFail = DateTime.MinValue;
        private static string list = string.Empty;

        public bool IsTorExit(IPAddress address)
        {
            if (!IsListActual())
                UpdateList();

            bool isInList = IsInList(address);
            return isInList;
        }

        private bool IsListActual()
        {
            return !(string.IsNullOrWhiteSpace(list) || DateTime.UtcNow - lastListUpdate > TimeSpan.FromDays(1));
        }

        private void UpdateList()
        {
            if (DateTime.UtcNow - lastUpdateFail > TimeSpan.FromMinutes(30))
            {
                WebClient webClient = new WebClient();
                var listTask = webClient.DownloadStringTaskAsync(new Uri(torExitNodesListUrl));
                if (listTask.Wait(TimeSpan.FromSeconds(2)))
                {
                    list = listTask.Result;
                    lastListUpdate = DateTime.UtcNow;
                }
                else
                {
                    lastUpdateFail = DateTime.UtcNow;
                }
            }
        }

        private bool IsInList(IPAddress address)
        {
            return list.Contains(address.ToString());
        }
    }
}
