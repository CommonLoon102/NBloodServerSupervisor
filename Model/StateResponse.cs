using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    [Serializable]
    public class StateResponse
    {
        public IList<Server> Servers { get; set; }
    }
}
