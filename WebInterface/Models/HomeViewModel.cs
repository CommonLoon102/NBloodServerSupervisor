using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface
{
    public class HomeViewModel
    {
        public IEnumerable<Server> Servers { get; }
        public string RunningSinceUtc { get; }
        public string ManHoursPlayed { get; }

        public HomeViewModel(IEnumerable<Server> servers, DateTime runningSinceUtc, int manHoursPlayed)
        {
            Servers = servers;
            RunningSinceUtc = runningSinceUtc.ToString("r");
            ManHoursPlayed = (manHoursPlayed / 60f).ToString("n2", CultureInfo.InvariantCulture);
        }
    }
}
