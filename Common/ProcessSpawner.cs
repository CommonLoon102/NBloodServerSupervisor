using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;

namespace Common
{
    public static class ProcessSpawner
    {
        private const int minPort = 23581;
        private const int maxPort = 23700;
        private const int maximumServers = 80;
        private const string nBloodExecutable = "nblood_server";

        private static readonly Random rnd = new Random();

        public static SpawnedServerInfo SpawnServer(int players, string modName, string tempFolderName = "")
        {
            int serversRunning = Process.GetProcessesByName(nBloodExecutable).Count();
            if (serversRunning >= maximumServers)
                throw new Exception("The maximum number of servers are already running.");

            Mod mod = GetModByName(modName);
            int port = GetPort();

            var process = Process.Start(GetProcessStartInfo(players, port, mod, tempFolderName));
            return new SpawnedServerInfo(process, port, mod);
        }

        private static Mod GetModByName(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName))
                return Constants.SupportedMods["BLOOD"];

            if (!Constants.SupportedMods.ContainsKey(modName.ToUpper()))
                throw new Exception($"This mod is not supported: {modName}.");

            return Constants.SupportedMods[modName.ToUpper()];
        }

        private static int GetPort()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var usedPorts = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port).ToList();
            usedPorts.AddRange(ipGlobalProperties.GetActiveTcpListeners().Select(c => c.Port).ToList());
            usedPorts.AddRange(ipGlobalProperties.GetActiveUdpListeners().Select(c => c.Port).ToList());

            var availablePorts = Enumerable.Range(minPort, maxPort - minPort + 1).ToList().Except(usedPorts).ToList();

            if (availablePorts.Count == 0)
                throw new Exception($"Cannot obtain free port in range {minPort}-{maxPort}.");

            int index = rnd.Next(0, availablePorts.Count() - 1);
            int port = availablePorts[index];
            return port;
        }

        private static ProcessStartInfo GetProcessStartInfo(int maxPlayers, int port, Mod mod, string tempFolderName = "")
        {
            string cmd = GetCommand(maxPlayers, port, mod, tempFolderName);

            var psi = new ProcessStartInfo(GetExecutableName(), cmd)
            {
                UseShellExecute = true,
                WorkingDirectory = CommandLineUtils.BloodDir
            };

            return psi;
        }

        private static string GetCommand(int maxPlayers, int port, Mod mod, string tempFolderName)
        {
            string cmd = $"-server {maxPlayers} -port {port} -pname Server {mod.CommandLine}";
            if (!string.IsNullOrWhiteSpace(tempFolderName))
            {
                string tempFolderPath = Path.Combine(CommandLineUtils.TempMapDir, tempFolderName);
                cmd += $" -j={tempFolderPath}";
            }

            return cmd;
        }

        private static string GetExecutableName()
        {
            string nbloodServer = nBloodExecutable;
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWindows)
                nbloodServer += ".exe";

            return nbloodServer;
        }
    }
}
