using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebInterface
{
    [RequestFormLimits(MultipartBodyLengthLimit = Constants.FileSizeLimit)]
    [RequestSizeLimit(Constants.FileSizeLimit)]
    public class PrivateViewModel
    {
        [Required]
        [Display(Name = "Required players")]
        public int Players { get; set; }
        [Required]
        [Display(Name = "Select mod")]
        public string ModName { get; set; }
        public List<SelectListItem> ModNames { get; } = Constants.SupportedMods
            .Select(m => new SelectListItem(m.Value.FriendlyName, m.Value.Name))
            .ToList();

        [Display(Name = "Custom map (optional)")]
        public IFormFile FormFile { get; set; }
    }
}
