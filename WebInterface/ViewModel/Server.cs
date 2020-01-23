using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface
{
    public class Server
    {
        public DateTime SpawnedAtUtc { get; set; } = DateTime.UtcNow;
        public int Port { get; set; }
        public bool IsStarted { get; set; }
        public string CommandLine { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaximumPlayers { get; set; }
        public string GameType { get; set; }
        public IList<Player> Players { get; set; } = new List<Player>();
    }
}
