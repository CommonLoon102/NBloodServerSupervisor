using System;
using System.Collections.Generic;
using System.Text;

namespace Supervisor
{
    abstract class PacketData
    {
        public int Port { get; set; }
        public bool IsStarted { get; set; }
    }
}
