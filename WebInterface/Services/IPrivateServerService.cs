using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public interface IPrivateServerService
    {
        SpawnedServerInfo SpawnNewPrivateServer(int players, string modName);
    }
}
