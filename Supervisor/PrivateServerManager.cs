using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Common;

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
                    TempMapFolderCleanup();
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    KillUnusedServers();
                }
            });
        }

        private static void KillUnusedServers()
        {
            try
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
            catch
            {
                // Log...
            }
        }

        private static void TempMapFolderCleanup()
        {
            try
            {
                if (!Directory.Exists(CommandLineUtils.TempMapDir))
                {
                    Directory.CreateDirectory(CommandLineUtils.TempMapDir);
                }

                foreach (var dir in Directory.GetDirectories(CommandLineUtils.TempMapDir))
                {
                    if (DateTime.UtcNow - File.GetCreationTimeUtc(dir) > TimeSpan.FromDays(1))
                    {
                        Directory.Delete(dir, recursive: true);
                    }
                }
            }
            catch
            {
                // Log...
            }
        }
    }
}
