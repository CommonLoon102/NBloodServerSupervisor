using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class State
    {
        public ConcurrentDictionary<int, Server> Servers { get; } = new ConcurrentDictionary<int, Server>();
    }
}
