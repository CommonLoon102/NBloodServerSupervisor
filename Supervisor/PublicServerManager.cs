using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor
{
    class PublicServerManager
    {
        public static void Start(string nbloodPath)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            KillOrphanedServers();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    LaunchNewServersWhenNeeded(nbloodPath);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            });
        }

        private static void KillOrphanedServers()
        {
            foreach (var process in Process.GetProcessesByName("nblood_server"))
            {
                if (!Program.State.Servers.Values.Any(s => s.ProcessId == process.Id))
                {
                    process.Kill();
                }
            }
        }

        private static void LaunchNewServersWhenNeeded(string nbloodPath)
        {
            const int maxPlayers = 8;

            for (int i = 3; i <= maxPlayers; i++)
            {
                if (IsNewServerNeeded(i))
                {
                    int port = PortUtils.GetPort();
                    var process = Process.Start(nbloodPath, $"-server {i} -port {port}");
                    Program.State.Servers.AddOrUpdate(port, new Server()
                    {
                        Port = port,
                        ProcessId = process.Id,
                        MaximumPlayers = i,
                        CurrentPlayers = 1,
                    },
                    (prt, server) =>
                    {
                        server.ProcessId = process.Id;
                        return server;
                    });
                }

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        private static bool IsNewServerNeeded(int i)
        {
            return !Program.State.Servers.Values.Any(s =>
                !s.IsPrivate && s.MaximumPlayers == i && s.CurrentPlayers < s.MaximumPlayers);
        }
    }
}
