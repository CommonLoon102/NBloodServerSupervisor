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
        private readonly IListServersService _listServersService;

        private static readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly IPEndPoint webApiListenerEndPoint = new IPEndPoint(IPAddress.Loopback, 11028);

        public NBloodController(ILogger<NBloodController> logger, IConfiguration config, IListServersService listServersService)
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

                string processName = Constants.NBloodExecutable;
                int serversRunning = Process.GetProcessesByName(processName).Count();
                if (serversRunning >= _config.GetValue<int>("MaximumServers"))
                    return new StartServerResponse("The maximum number of servers are already running.");

                Mod mod = GetMod(parameters.ModName);
                int port = PortUtils.GetPort();

                var process = Process.Start(NBloodServerStartInfo.Get(parameters.Players, port, mod));
                byte[] payload = Encoding.ASCII.GetBytes($"B{port}\t{process.Id}\0");
                socket.SendTo(payload, webApiListenerEndPoint);

                _logger.LogInformation("Server started waiting for {0} players on port {1}.",
                    parameters.Players, port);

                Thread.Sleep(TimeSpan.FromSeconds(2));
                return new StartServerResponse(port) { CommandLine = CommandLineUtils.GetLaunchCommand(HttpContext.Request.Host.Host, port, mod) };
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

        private Mod GetMod(string modName)
        {
            if (string.IsNullOrWhiteSpace(modName))
                return Constants.SupportedMods["BLOOD"];

            if (!Constants.SupportedMods.ContainsKey(modName.ToUpper()))
                throw new Exception("This mod is not supported: " + modName);

            return Constants.SupportedMods[modName.ToUpper()];
        }
    }
}