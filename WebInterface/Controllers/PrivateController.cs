using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebInterface.Infrastructure;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    public class PrivateController : Controller
    {
        private readonly IPrivateServerService _privateServerService;
        private readonly IRateLimiterService _rateLimiterService;
        private readonly ICustomMapService _customMapService;
        private readonly ILogger<PrivateController> _logger;

        public PrivateController(IPrivateServerService privateServerService,
            IRateLimiterService rateLimiterService,
            ICustomMapService customMapService,
            ILogger<PrivateController> logger)
        {
            _privateServerService = privateServerService;
            _rateLimiterService = rateLimiterService;
            _customMapService = customMapService;
            _logger = logger;
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
                    throw new WebInterfaceException("Something went off the rails.");

                if (!_rateLimiterService.IsRequestAllowed(HttpContext.Connection.RemoteIpAddress))
                    throw new WebInterfaceException("Sorry, you have requested too many servers recently, you need to wait some time.");

                string tempFolderName = "";
                if ((request.FormFile?.Length ?? 0) > 0)
                    tempFolderName = _customMapService.StoreTempCustomMap(request.FormFile);

                var spawnedServer = _privateServerService.SpawnNewPrivateServer(request.Players + 1, request.ModName, tempFolderName ?? "");
                string commandLine = CommandLineUtils.GetClientLaunchCommand(HttpContext.Request.Host.Host,
                    spawnedServer.Port,
                    spawnedServer.Mod.CommandLine);

                viewModel = new StartServerResponse(spawnedServer.Port, commandLine);
            }
            catch (WebInterfaceException ex)
            {
                viewModel = new StartServerResponse(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                viewModel = new StartServerResponse("Internal server error.");
            }

            return View("Result", viewModel);
        }
    }
}