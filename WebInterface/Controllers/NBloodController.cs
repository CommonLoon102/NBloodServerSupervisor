using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;

namespace WebInterface.Controllers
{
    [ApiController]
    public class NBloodController : ControllerBase
    {
        private static bool _isBusy = false;
        private static DateTime _lastRefresh = DateTime.MinValue;
        private static ListServersResponse _lastServerList = null;
        private static readonly object _locker = new object();

        private readonly ILogger<NBloodController> _logger;
        private readonly IConfiguration _config;

        private const int listenPort = 11029;
        private static readonly IPEndPoint remoteIP = new IPEndPoint(IPAddress.Loopback, listenPort);
        private static readonly UdpClient udpClient = new UdpClient(remoteIP);

        private static readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static readonly IPEndPoint webApiListenerEndPoint = new IPEndPoint(IPAddress.Loopback, 11028);

        public NBloodController(ILogger<NBloodController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
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
                return new StartServerResponse(port) { CommandLine = GetCommandLine(port, mod) };
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
                    byte[] payload = Encoding.ASCII.GetBytes($"A");
                    byte[] response;
                    lock (_locker)
                    {
                        socket.SendTo(payload, webApiListenerEndPoint);
                        response = udpClient.ReceiveAsync().Result.Buffer;
                    }

                    StateResponse stateResponse = (StateResponse)ByteArrayToObject(response);
                    var webResponse = new ListServersResponse
                    {
                        Servers = stateResponse.Servers.Where(s => !s.IsPrivate).Select(s => new Server()
                        {
                            Port = s.Port,
                            IsStarted = s.IsStarted,
                            CommandLine = s.CurrentPlayers == s.MaximumPlayers ? "Sorry, the game is already started." : GetCommandLine(s.Port, s.Mod),
                            GameType = s.GameType,
                            Mod = s.Mod.FriendlyName,
                            CurrentPlayers = s.CurrentPlayers,
                            MaximumPlayers = s.MaximumPlayers,
                            Players = s.Players.Select(p => new Player() { Name = p.Name, Score = p.Score }).ToList(),
                            SpawnedAtUtc = s.SpawnedAtUtc
                        }).OrderBy(s => s.MaximumPlayers).ToList()
                    };

                    _lastServerList = webResponse;
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

        private static object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        private string GetCommandLine(int port, Mod mod)
        {
            return $"nblood -client {HttpContext.Request.Host.Host} -port {port} {mod.CommandLine}";
        }
    }
}