using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Common
{
    public static class NBloodServerStartInfo
    {
        private static readonly string workingDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "blood");

        public static ProcessStartInfo Get(int maxPlayers, int port, Mod mod)
        {
            var psi = new ProcessStartInfo(GetExecutable(), $"-server {maxPlayers} -port {port} -pname Server {mod.CommandLine}")
            {
                UseShellExecute = true,
                WorkingDirectory = workingDir
            };

            return psi;
        }

        private static string GetExecutable()
        {
            string nbloodServer = Constants.NBloodExecutable;
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (isWindows)
                nbloodServer += ".exe";

            return nbloodServer;
        }
    }
}
