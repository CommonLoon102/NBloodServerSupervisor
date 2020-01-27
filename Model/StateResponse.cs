using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    [Serializable]
    public class StateResponse
    {
        public IList<Server> Servers { get; set; }
        public int ManMinutesPlayed { get; set; }
        public DateTime RunningSinceUtc { get; set; }
    }
}
