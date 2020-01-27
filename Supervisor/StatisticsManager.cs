using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor
{
    class StatisticsManager
    {
        public static void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    UpdatePlaytime();
                }
            });
        }

        private static void UpdatePlaytime()
        {
            TimeSpan timeToAdd = new TimeSpan(0);
            var now = DateTime.UtcNow;
            foreach (var server in Program.State.Servers.Values.Where(s => s.IsStarted))
            {
                if (server.LastCollectionUtc.HasValue)
                {
                    var elapsedTime = now - server.LastCollectionUtc.Value;
                    timeToAdd += elapsedTime * (server.CurrentPlayers - 1);
                }

                server.LastCollectionUtc = now;
            }

            Program.State.IncreasePlaytime(timeToAdd);
        }
    }
}
