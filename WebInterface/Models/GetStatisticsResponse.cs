using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface
{
    public class GetStatisticsResponse
    {
        public DateTime RunningSinceUtc { get; set; }
        public int ManMinutesPlayed { get; set; }
    }
}
