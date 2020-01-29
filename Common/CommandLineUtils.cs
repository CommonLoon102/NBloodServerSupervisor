using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Common
{
    public static class CommandLineUtils
    {
        public static string BloodDir => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "blood");
        public static string TempMapDir => Path.Combine(BloodDir, "tempmaps");

        public static string GetClientLaunchCommand(string host, int port, string modCommandLine) =>
            $"nblood -client {host} -port {port} {modCommandLine}";
    }
}
