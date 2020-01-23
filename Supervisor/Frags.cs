using System;
using System.Collections.Generic;
using System.Text;

namespace Supervisor
{
    class Frags : PacketData
    {
        public IList<int> Scores { get; set; }
        public int GameType { get; set; }

        public Frags()
        {
            IsStarted = true;
        }
    }
}
