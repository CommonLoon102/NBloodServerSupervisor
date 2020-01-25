using System;
using System.Collections.Generic;
using System.Text;

namespace Supervisor
{
    class PlayerNames : PacketData
    {
        public IList<string> Names { get; set; }

        public PlayerNames()
        {
            IsStarted = true;
        }
    }
}
