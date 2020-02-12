using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebInterface.Services
{
    public interface ICustomMapService
    {
        IReadOnlyList<string> ListCustomMaps();
        byte[] GetCustomMapBytes(string map);
        string StoreTempCustomMap(IFormFile formFile);
    }
}
