using System;
using System.Collections.Generic;

namespace Model
{
    [Serializable]
    public class Server
    {
        public DateTime SpawnedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastCollectionUtc { get; set; }
        public DateTime LastHeartBeatUtc { get; set; }
        public int ProcessId { get; set; }
        public int Port { get; set; }
        public bool IsStarted { get; set; }
        public bool IsPrivate { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaximumPlayers { get; set; }
        public string GameType { get; set; }
        public Mod Mod { get; set; }
        public IList<Player> Players { get; set; } = new List<Player>();
    }
}
