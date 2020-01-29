using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public class PrivateServerService : IPrivateServerService
    {
        private static readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly IPEndPoint webApiListenerEndPoint = new IPEndPoint(IPAddress.Loopback, 11028);

        public SpawnedServerInfo SpawnNewPrivateServer(int players, string modName, string tempFolderName = "")
        {
            players = Math.Min(8, Math.Max(3, players));

            SpawnedServerInfo serverProcess = ProcessSpawner.SpawnServer(players, modName, tempFolderName);
            byte[] payload = Encoding.ASCII.GetBytes($"B{serverProcess.Port}\t{serverProcess.Process.Id}\0");
            socket.SendTo(payload, webApiListenerEndPoint);

            return serverProcess;
        }
    }
}
