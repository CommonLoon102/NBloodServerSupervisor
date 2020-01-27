using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class State
    {
        private TimeSpan playtime = new TimeSpan(0);

        public ConcurrentDictionary<int, Server> Servers { get; } = new ConcurrentDictionary<int, Server>();
        public DateTime CreatedAtUtc { get; } = DateTime.UtcNow;
        public TimeSpan Playtime => playtime;

        public void IncreasePlaytime(TimeSpan timeToAdd)
        {
            playtime += timeToAdd;
        }
    }
}
