using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Supervisor
{
    static class PrivateServerManager
    {
        public static void Start()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    KillUnusedServers();
                }
            });
        }

        private static void KillUnusedServers()
        {
            var killables = Program.State.Servers.Values.Where(s => 
                s.IsPrivate
                && !s.IsStarted
                && s.CurrentPlayers < 2
                && (DateTime.UtcNow - s.SpawnedAtUtc) > TimeSpan.FromMinutes(10));

            foreach (var server in killables)
            {
                Process.GetProcessById(server.ProcessId).Kill();
            }
        }
    }
}
