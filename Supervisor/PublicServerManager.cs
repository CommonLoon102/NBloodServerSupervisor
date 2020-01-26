using Common;
using Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Supervisor
{
    static class PublicServerManager
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
            foreach (Mod mod in Constants.SupportedMods.Values)
            {
                const int maxPlayers = 8;
                for (int i = 3; i <= maxPlayers; i++)
                {
                    if (IsNewServerNeeded(i, mod))
                    {
                        int port = PortUtils.GetPort();
                        var process = Process.Start(NBloodServerStartInfo.Get(i, port, mod));
                        Program.State.Servers.AddOrUpdate(port, new Server()
                        {
                            Port = port,
                            ProcessId = process.Id,
                            MaximumPlayers = i,
                            CurrentPlayers = 1,
                            Mod = mod,
                        },
                        (prt, server) =>
                        {
                            server.ProcessId = process.Id;
                            server.MaximumPlayers = i;
                            server.CurrentPlayers = 1;
                            server.Mod = mod;
                            return server;
                        });
                    }
                }
            }
        }

        private static bool IsNewServerNeeded(int i, Mod mod)
        {
            return !Program.State.Servers.Values.Any(s =>
                !s.IsPrivate
                && s.Mod.Name == mod.Name
                && s.MaximumPlayers == i
                && s.CurrentPlayers < s.MaximumPlayers);
        }
    }
}
