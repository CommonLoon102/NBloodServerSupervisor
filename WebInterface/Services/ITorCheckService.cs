using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public interface ITorCheckService
    {
        bool IsTorExit(IPAddress address);
    }
}
