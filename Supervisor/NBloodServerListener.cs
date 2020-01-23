using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Supervisor
{
    static class NBloodServerListener
    {
        private const int listenPort = 11027;
        private static IPEndPoint remoteIP = new IPEndPoint(IPAddress.Loopback, listenPort);
        private static UdpClient udpClient = new UdpClient(remoteIP);

        public static async void StartListening()
        {
            while (true)
            {
                UdpReceiveResult rst = await udpClient.ReceiveAsync();
                ProcessPacket(rst.Buffer);
            }
        }

        private static void ProcessPacket(byte[] buffer)
        {
            string message = Encoding.ASCII.GetString(buffer);
            switch (message[0])
            {
                case 'A':
                    ProcessPlayerCountsPacket(message);
                    break;
                case 'B':
                    ProcessPlayerNamesPacket(message);
                    break;
                case 'C':
                    ProcessFragsPacket(buffer);
                    break;
                default:
                    break;
            }
        }

        private static void ProcessPlayerCountsPacket(string message)
        {
            string[] splitMessage = message.SplitMessage();
            var packetData = new PlayerCounts()
            {
                Port = int.Parse(splitMessage[0]),
                IsStarted = int.Parse(splitMessage[1]) != 0,
                CurrentPlayers = int.Parse(splitMessage[2]),
                MaximumPlayers = int.Parse(splitMessage[3])
            };

            UpdateState(packetData);
        }

        private static void UpdateState(PlayerCounts packetData)
        {
            Program.State.Servers.AddOrUpdate(packetData.Port,
                new Server()
                {
                    Port = packetData.Port,
                    IsStarted = packetData.IsStarted,
                    IsPrivate = packetData.IsPrivate,
                    CurrentPlayers = packetData.CurrentPlayers,
                    MaximumPlayers = packetData.MaximumPlayers
                },
                (port, server) =>
                {
                    server.IsStarted = packetData.IsStarted;
                    server.IsPrivate = packetData.IsPrivate;
                    server.CurrentPlayers = packetData.CurrentPlayers;
                    server.MaximumPlayers = packetData.MaximumPlayers;
                    return server;
                });
        }

        private static void ProcessPlayerNamesPacket(string message)
        {
            string[] splitMessage = message.SplitMessage();
            var packetData = new PlayerNames()
            {
                Port = int.Parse(splitMessage[0]),
                Names = splitMessage.Skip(1).ToList(),
            };

            UpdateState(packetData);
        }

        private static void UpdateState(PlayerNames packetData)
        {
            Program.State.Servers.AddOrUpdate(packetData.Port,
            new Server()
            {
                Port = packetData.Port,
                IsStarted = packetData.IsStarted,
                Players = packetData.Names.Select(name => new Player() { Name = name }).ToList()
            },
            (port, server) =>
            {
                server.IsStarted = packetData.IsStarted;
                if (server.Players.Count == 0 || server.CurrentPlayers != packetData.Names.Count)
                {
                    server.CurrentPlayers = packetData.Names.Count;
                    server.Players = packetData.Names.Select(name => new Player()
                    {
                        Name = name,
                        Score = 0
                    }).ToList();
                }
                return server;
            });
        }

        private static void ProcessFragsPacket(byte[] buffer)
        {
            int port = BitConverter.ToInt32(buffer, 4);
            int gameType = BitConverter.ToInt32(buffer, 8);
            const int maxPlayers = 8;
            int[] scores = new int[maxPlayers];
            for (int i = 0; i < scores.Length; i++)
                scores[i] = BitConverter.ToInt32(buffer, 12 + i * sizeof(int));

            var packetData = new Frags()
            {
                Port = port,
                GameType = gameType,
                Scores = scores
            };

            UpdateState(packetData);
        }

        private static void UpdateState(Frags packetData)
        {
            Program.State.Servers.AddOrUpdate(packetData.Port,
            new Server()
            {
                Port = packetData.Port,
                IsStarted = packetData.IsStarted,
                GameType = GetGameType(packetData.GameType)
            },
            (port, server) =>
            {
                server.IsStarted = packetData.IsStarted;
                server.GameType = GetGameType(packetData.GameType);
                for (int i = 0; i < server.CurrentPlayers; i++)
                    server.Players[i].Score = packetData.Scores[i];
                return server;
            });
        }

        private static string GetGameType(int gameType)
        {
            return gameType switch
            {
                1 => "Cooperative",
                2 => "Bloodbath",
                3 => "Capture The Flag",
                _ => "Unknown",
            };
        }
    }
}
