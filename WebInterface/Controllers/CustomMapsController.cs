using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebInterface.Services;

namespace WebInterface.Controllers
{
    public class CustomMapsController : Controller
    {
        private readonly ICustomMapService _customMapService;

        public CustomMapsController(ICustomMapService customMapService)
        {
            _customMapService = customMapService;
        }

        [Route("nblood/custommaps", Name = "CustomMaps")]
        public IActionResult Index()
        {
            var viewModel = _customMapService.ListCustomMaps();
            return View(viewModel);
        }

        [Route("nblood/custommaps/download", Name = "DownloadCustomMap")]
        public FileResult Index([FromQuery] string map)
        {
            byte[] customMap = _customMapService.GetCustomMapBytes(map);
            return File(customMap, System.Net.Mime.MediaTypeNames.Application.Octet, map);
        }
    }
}