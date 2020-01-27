using Common;
using Model;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace WebInterface.Services
{
    public class StateService : IStateService
    {
        private const int listenPort = 11029;

        private static readonly object _locker = new object();
        private static readonly IPEndPoint remoteIP = new IPEndPoint(IPAddress.Loopback, listenPort);
        private static readonly UdpClient udpClient = new UdpClient(remoteIP);

        private static readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly IPEndPoint webApiListenerEndPoint = new IPEndPoint(IPAddress.Loopback, 11028);

        public ListServersResponse ListServers(string host)
        {
            StateResponse stateResponse = RequestState();
            var serversResponse = new ListServersResponse
            {
                Servers = stateResponse.Servers.Where(s => !s.IsPrivate).Select(s => new Server()
                {
                    Port = s.Port,
                    IsStarted = s.IsStarted,
                    CommandLine = GetCommandLine(s, host),
                    GameType = s.GameType,
                    Mod = s.Mod.FriendlyName,
                    CurrentPlayers = s.CurrentPlayers,
                    MaximumPlayers = s.MaximumPlayers,
                    Players = s.Players.Select(p => new Player() { Name = p.Name, Score = p.Score }).ToList(),
                    SpawnedAtUtc = s.SpawnedAtUtc
                }).OrderBy(s => s.MaximumPlayers).ToList()
            };

            return serversResponse;
        }

        public string GetCommandLine(Model.Server server, string host)
        {
            if (server.CurrentPlayers == server.MaximumPlayers)
                return "Sorry, the game is already started.";

            return CommandLineUtils.GetClientLaunchCommand(host, server.Port, server.Mod.CommandLine);
        }

        public GetStatisticsResponse GetStatistics()
        {
            StateResponse stateResponse = RequestState();
            var statisticsResponse = new GetStatisticsResponse()
            {
                ManMinutesPlayed = stateResponse.ManMinutesPlayed,
                RunningSinceUtc = stateResponse.RunningSinceUtc
            };

            return statisticsResponse;
        }

        private static StateResponse RequestState()
        {
            byte[] payload = Encoding.ASCII.GetBytes($"A");
            byte[] response;
            lock (_locker)
            {
                socket.SendTo(payload, webApiListenerEndPoint);
                response = udpClient.ReceiveAsync().Result.Buffer;
            }

            StateResponse stateResponse = (StateResponse)ByteArrayToObject(response);
            return stateResponse;
        }

        private static object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}
