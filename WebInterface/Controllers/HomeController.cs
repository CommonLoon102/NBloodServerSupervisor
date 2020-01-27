using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    public class HomeController : Controller
    {
        IStateService _stateService;

        public HomeController(IStateService stateService)
        {
            _stateService = stateService;
        }

        [Route("nblood/home", Name = "Home")]
        public IActionResult Index()
        {
            var servers = _stateService.ListServers(HttpContext.Request.Host.Host).Servers;
            var stats = _stateService.GetStatistics();

            var viewModel = new HomeViewModel(servers, stats.RunningSinceUtc, stats.ManMinutesPlayed);
            return View(viewModel);
        }
    }
}