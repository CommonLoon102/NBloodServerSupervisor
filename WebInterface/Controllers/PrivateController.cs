using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    public class PrivateController : Controller
    {
        private readonly IPrivateServerService _privateServerService;
        private readonly IRateLimiterService _rateLimiterService;

        public PrivateController(IPrivateServerService privateServerService, IRateLimiterService rateLimiterService)
        {
            _privateServerService = privateServerService;
            _rateLimiterService = rateLimiterService;
        }

        [Route("nblood/private")]
        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new PrivateViewModel
            {
                ModName = Constants.SupportedMods.Values.First().Name,
                Players = 2
            };

            return View(viewModel);
        }

        [Route("nblood/private", Name = "RequestPrivateServer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(PrivateViewModel request)
        {
            StartServerResponse viewModel;
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("Something went off the rails.");

                if (!_rateLimiterService.IsRequestAllowed(HttpContext.Connection.RemoteIpAddress))
                    throw new Exception("Sorry, you have requested too many servers recently, you need to wait some time.");

                var spawnedServer = _privateServerService.SpawnNewPrivateServer(request.Players + 1, request.ModName);
                string commandLine = CommandLineUtils.GetClientLaunchCommand(HttpContext.Request.Host.Host,
                    spawnedServer.Port,
                    spawnedServer.Mod.CommandLine);

                viewModel = new StartServerResponse(spawnedServer.Port, commandLine);
            }
            catch (Exception ex)
            {
                viewModel = new StartServerResponse(ex.Message);
            }

            return View("Result", viewModel);
        }
    }
}