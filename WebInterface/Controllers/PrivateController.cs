using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    public class PrivateController : Controller
    {
        private readonly IPrivateServerService _privateServerService;
        private readonly IRateLimiterService _rateLimiterService;
        private readonly ICustomMapService _customMapService;

        public PrivateController(IPrivateServerService privateServerService,
            IRateLimiterService rateLimiterService,
            ICustomMapService customMapService)
        {
            _privateServerService = privateServerService;
            _rateLimiterService = rateLimiterService;
            _customMapService = customMapService;
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

                string tempFolderName = "";
                if ((request.FormFile?.Length ?? 0) > 0)
                     tempFolderName = _customMapService.StoreTempCustomMap(request.FormFile);

                var spawnedServer = _privateServerService.SpawnNewPrivateServer(request.Players + 1, request.ModName, tempFolderName ?? "");
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