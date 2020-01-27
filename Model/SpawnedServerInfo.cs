using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Model
{
    public class SpawnedServerInfo
    {
        public Process Process { get; }
        public int Port { get; }
        public Mod Mod { get; }

        public SpawnedServerInfo(Process process, int port, Mod mod)
        {
            Process = process;
            Port = port;
            Mod = mod;
        }
    }
}
