using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebInterface.Services
{
    public interface ICustomMapService
    {
        IList<string> ListCustomMaps();
        byte[] GetCustomMapBytes(string map);
        string StoreTempCustomMap(IFormFile formFile);
    }
}
