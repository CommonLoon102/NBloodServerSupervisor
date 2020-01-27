using System;
using System.Linq;
using System.Threading;
using Model;

namespace Supervisor
{
    class Program
    {
        public static readonly State State = new State();

        public static void Main()
        {
            NBloodServerListener.StartListening();
            WebApiListener.StartListening();
            PublicServerManager.Start();
            PrivateServerManager.Start();
            StatisticsManager.Start();

            while (true)
            {
                RemoveCrashedServers();
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        private static void RemoveCrashedServers()
        {
            var crashedServers = State.Servers.Values
                .Where(s => s.IsStarted && DateTime.UtcNow - s.LastHeartBeatUtc < TimeSpan.FromMinutes(15))
                .Select(s => s.Port);

            foreach (var port in crashedServers)
            {
                State.Servers.TryRemove(port, out _);
            }
        }
    }
}
