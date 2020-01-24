using Common;
using Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor
{
    class PublicServerManager
    {
        public static void Start()
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            KillOrphanedServers();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    LaunchNewServersWhenNeeded();
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

        private static void LaunchNewServersWhenNeeded()
        {
            const int maxPlayers = 8;

            for (int i = 3; i <= maxPlayers; i++)
            {
                if (IsNewServerNeeded(i))
                {
                    int port = PortUtils.GetPort();
                    string nbloodServer = "nblood_server";
                    bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                    if (isWindows)
                        nbloodServer += ".exe";
                    var process = Process.Start(nbloodServer, $"-server {i} -port {port}");
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
