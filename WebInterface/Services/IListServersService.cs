using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public interface IListServersService
    {
        ListServersResponse ListServers(string host);
    }
}
