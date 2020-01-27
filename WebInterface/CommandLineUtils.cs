using Microsoft.AspNetCore.Http;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface
{
    public static class CommandLineUtils
    {
        public static string GetLaunchCommand(string host, int port, Mod mod)
        {
            return $"nblood -client {host} -port {port} {mod.CommandLine}";
        }
    }
}
