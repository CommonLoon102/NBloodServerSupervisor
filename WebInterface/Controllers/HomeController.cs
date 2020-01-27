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
        IListServersService _serversList;

        public HomeController(IListServersService serversList)
        {
            _serversList = serversList;
        }

        [Route("nblood/home")]
        public IActionResult Index()
        {
            var viewModel = _serversList.ListServers(HttpContext.Request.Host.Host).Servers;
            return View(viewModel);
        }
    }
}