using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace Common
{
    public class PortUtils
    {
        public static int GetPort()
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var usedPorts = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port).ToList();
            usedPorts.AddRange(ipGlobalProperties.GetActiveTcpListeners().Select(c => c.Port).ToList());
            usedPorts.AddRange(ipGlobalProperties.GetActiveUdpListeners().Select(c => c.Port).ToList());

            var availablePorts = Enumerable.Range(1025, ushort.MaxValue).ToList().Except(usedPorts).ToList();

            Random rnd = new Random();
            int index = rnd.Next(0, availablePorts.Count() - 1);
            int port = availablePorts[index];
            return port;
        }
    }
}
