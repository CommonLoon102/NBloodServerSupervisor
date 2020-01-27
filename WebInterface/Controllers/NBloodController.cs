using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    [ApiController]
    public class NBloodController : ControllerBase
    {
        private static bool _isBusy = false;
        private static DateTime _lastRefresh = DateTime.MinValue;
        private static ListServersResponse _lastServerList = null;

        private readonly ILogger<NBloodController> _logger;
        private readonly IConfiguration _config;
        private readonly IStateService _listServersService;

        private static readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly IPEndPoint webApiListenerEndPoint = new IPEndPoint(IPAddress.Loopback, 11028);

        public NBloodController(ILogger<NBloodController> logger, IConfiguration config, IStateService listServersService)
        {
            _logger = logger;
            _config = config;
            _listServersService = listServersService;
        }

        [HttpGet]
        [Route("[controller]/api/startserver")]
        public StartServerResponse StartServer([FromQuery] ServerParameters parameters)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                while (_isBusy)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (sw.Elapsed.TotalSeconds > 5)
                        throw new Exception("Request timeout: the previous request hasn't finished yet.");
                }

                _isBusy = true;

                if (parameters.Players < 3)
                    parameters.Players = 3;

                if (parameters.ApiKey != _config.GetValue<string>("ApiKey"))
                    return new StartServerResponse("Invalid ApiKey.");

                SpawnedServerInfo serverProcess = ProcessSpawner.SpawnServer(parameters.Players, parameters.ModName);
                byte[] payload = Encoding.ASCII.GetBytes($"B{serverProcess.Port}\t{serverProcess.Process.Id}\0");
                socket.SendTo(payload, webApiListenerEndPoint);

                _logger.LogInformation("Server started waiting for {0} players on port {1}.",
                    parameters.Players, serverProcess.Port);

                Thread.Sleep(TimeSpan.FromSeconds(2));
                return new StartServerResponse(serverProcess.Port)
                {
                    CommandLine = CommandLineUtils.GetClientLaunchCommand(HttpContext.Request.Host.Host,
                        serverProcess.Port,
                        serverProcess.Mod.CommandLine)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new StartServerResponse("Unhandled exception has been occured. Check the logs for details.");
            }
            finally
            {
                _isBusy = false;
            }
        }

        [HttpGet]
        [Route("[controller]/api/listservers")]
        public ListServersResponse ListServers()
        {
            try
            {
                if (DateTime.UtcNow - _lastRefresh > TimeSpan.FromSeconds(1)
                    || _lastServerList == null)
                {
                    _lastServerList = _listServersService.ListServers(HttpContext.Request.Host.Host);
                    _lastRefresh = DateTime.UtcNow;
                }

                return _lastServerList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ListServersResponse("Unhandled exception has been occured. Check the logs for details.");
            }
        }
    }
}