using System;
using System.Collections.Generic;
using System.Text;

namespace Supervisor
{
    class RemoveServer : PacketData
    {
        public RemoveServer()
        {
            IsStarted = false;
        }
    }
}
