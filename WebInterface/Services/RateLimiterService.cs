using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public class RateLimiterService : IRateLimiterService
    {
        private static readonly ConcurrentDictionary<string, List<DateTime>> requestHistory = new ConcurrentDictionary<string, List<DateTime>>();

        public bool IsRequestAllowed(IPAddress remoteIpAddress)
        {
            string hash = GetHash(remoteIpAddress);
            if (requestHistory.TryGetValue(hash, out var list))
            {
                 bool isLimited = list.Count(e => DateTime.UtcNow - e < TimeSpan.FromHours(1)) >= 5;
                if (isLimited)
                    return false;
            }

            requestHistory.AddOrUpdate(hash,
                (ip) =>
                {
                    var list = new List<DateTime>();
                    list.Add(DateTime.UtcNow);
                    return list;
                },
                (ip, list) =>
                {
                    list.Add(DateTime.UtcNow);
                    return list;
                });

            return true;
        }

        private string GetHash(IPAddress remoteIpAddress)
        {
            const string pepper = "Somewhere, over the rainbow, way up this pepper.";
            var data = Encoding.ASCII.GetBytes(remoteIpAddress.ToString() + pepper);
            string hash;
            using (SHA512 shaM = new SHA512Managed())
            {
                byte[] bytes = shaM.ComputeHash(data);
                for (int i = 0; i < 1000; i++)
                {
                    bytes = shaM.ComputeHash(bytes);
                }

                bytes = bytes.Take(bytes.Length / 4).ToArray();
                hash = Encoding.ASCII.GetString(bytes);
            }

            return hash;
        }
    }
}
