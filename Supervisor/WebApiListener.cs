using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace Supervisor
{
    static class WebApiListener
    {
        private const int listenPort = 11028;
        private static readonly IPEndPoint remoteIP = new IPEndPoint(IPAddress.Loopback, listenPort);
        private static readonly UdpClient udpClient = new UdpClient(remoteIP);

        private static readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly IPEndPoint webApiEndPoint = new IPEndPoint(IPAddress.Loopback, 11029);

        public static async void StartListening()
        {
            while (true)
            {
                UdpReceiveResult rst = await udpClient.ReceiveAsync();
                ProcessWebApiMessage(rst.Buffer);
            }
        }

        private static void ProcessWebApiMessage(byte[] buffer)
        {
            string message = Encoding.ASCII.GetString(buffer);
            switch (message[0])
            {
                case 'A':
                    ProcessGetCurrentStateRequest();
                    break;
                case 'B':
                    StorePrivateServerInfo(message);
                    break;
                default:
                    break;
            }
        }

        private static void ProcessGetCurrentStateRequest()
        {
            var response = new StateResponse();
            response.Servers = Program.State.Servers.Values.ToList();

            byte[] serializedResponse = ObjectToByteArray(response);
            socket.SendTo(serializedResponse, webApiEndPoint);
        }

        private static void StorePrivateServerInfo(string message)
        {
            string[] split = message.SplitMessage();
            int port = int.Parse(split[0]);
            int processId = int.Parse(split[1]);
            Program.State.Servers.AddOrUpdate(port, new Server()
            {
                Port = port,
                ProcessId = processId,
                IsPrivate = true
            },
            (prt, server) =>
            {
                server.ProcessId = processId;
                server.IsPrivate = true;
                return server;
            });
        }

        private static byte[] ObjectToByteArray(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
