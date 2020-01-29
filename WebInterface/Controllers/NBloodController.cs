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
        private const string generalErrorMessage = "Unhandled exception has been occured. Check the server logs for details.";

        private static bool _isBusy = false;
        private static DateTime _lastRefresh = DateTime.MinValue;
        private static ListServersResponse _lastServerList = null;

        private readonly ILogger<NBloodController> _logger;
        private readonly IConfiguration _config;
        private readonly IStateService _stateService;
        private readonly IPrivateServerService _privateServerService;

        public NBloodController(ILogger<NBloodController> logger,
            IConfiguration config,
            IStateService stateService,
            IPrivateServerService privateServerService)
        {
            _logger = logger;
            _config = config;
            _stateService = stateService;
            _privateServerService = privateServerService;
        }

        [HttpGet]
        [Route("[controller]/api/startserver")]
        public StartServerResponse StartServer([FromQuery] ServerParameters parameters)
        {
            try
            {
                if (parameters.ApiKey != _config.GetValue<string>("ApiKey"))
                    return new StartServerResponse("Invalid ApiKey.");

                Stopwatch sw = Stopwatch.StartNew();
                while (_isBusy)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    if (sw.Elapsed.TotalSeconds > 5)
                        throw new Exception("Request timeout: the previous request hasn't finished yet.");
                }

                _isBusy = true;

                var serverProcess = _privateServerService.SpawnNewPrivateServer(parameters.Players, parameters.ModName);
                _logger.LogInformation("Server started waiting for {0} players on port {1}.",
                    parameters.Players, serverProcess.Port);

                string commandLine = CommandLineUtils.GetClientLaunchCommand(HttpContext.Request.Host.Host,
                    serverProcess.Port,
                    serverProcess.Mod.CommandLine);

                Thread.Sleep(TimeSpan.FromSeconds(2));
                return new StartServerResponse(serverProcess.Port, commandLine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new StartServerResponse(generalErrorMessage);
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
                    _lastServerList = _stateService.ListServers(HttpContext.Request.Host.Host);
                    _lastRefresh = DateTime.UtcNow;
                }

                return _lastServerList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ListServersResponse(generalErrorMessage);
            }
        }
    }
}