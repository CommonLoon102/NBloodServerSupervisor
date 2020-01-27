using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public interface IRateLimiterService
    {
        bool IsRequestAllowed(IPAddress remoteIpAddress);
    }
}
