using System;
using System.Collections.Generic;
using System.Text;

namespace Supervisor
{
    class PlayerCounts : PacketData
    {
        public int CurrentPlayers { get; set; }
        public int MaximumPlayers { get; set; }

        public PlayerCounts()
        {
            IsStarted = true;
        }
    }
}
