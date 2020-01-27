using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public class RateLimiterService : IRateLimiterService
    {
        private static readonly ConcurrentDictionary<IPAddress, List<DateTime>> requestHistory = new ConcurrentDictionary<IPAddress, List<DateTime>>();

        public bool IsRequestAllowed(IPAddress remoteIpAddress)
        {
            if (requestHistory.TryGetValue(remoteIpAddress, out var list))
            {
                 bool isLimited = list.Count(e => DateTime.UtcNow - e < TimeSpan.FromHours(1)) >= 5;
                if (isLimited)
                    return false;
            }

            requestHistory.AddOrUpdate(remoteIpAddress,
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
    }
}
