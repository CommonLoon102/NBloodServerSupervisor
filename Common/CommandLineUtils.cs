using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class CommandLineUtils
    {
        public static string GetClientLaunchCommand(string host, int port, string modCommandLine) =>
            $"nblood -client {host} -port {port} {modCommandLine}";
    }
}
