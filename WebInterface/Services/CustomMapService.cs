using Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebInterface.Infrastructure;

namespace WebInterface.Services
{
    public class CustomMapService : ICustomMapService
    {
        private static readonly List<string> crypticMaps = new List<string>()
        {
            "CPSL.MAP",
            "CP01.MAP",
            "CP02.MAP",
            "CP03.MAP",
            "CP04.MAP",
            "CP05.MAP",
            "CP06.MAP",
            "CP07.MAP",
            "CP08.MAP",
            "CP09.MAP",
            "CPBB01.MAP",
            "CPBB02.MAP",
            "CPBB03.MAP",
            "CPBB04.MAP",
        };

        private List<string> ListableCustomMaps => Directory.GetFiles(CommandLineUtils.BloodDir,
            "*.map", SearchOption.TopDirectoryOnly)
            .Select(m => Path.GetFileName(m))
            .Where(m => !ContainsString(crypticMaps, m))
            .ToList();

        public IList<string> ListCustomMaps() => ListableCustomMaps;

        public byte[] GetCustomMapBytes(string map)
        {
            if (ListableCustomMaps.Any(m => StringsAreSame(m, map)))
            {
                return File.ReadAllBytes(Path.Combine(CommandLineUtils.BloodDir, map));
            }
            else
            {
                throw new WebInterfaceException($"Cannot download this map: {map}");
            }
        }

        private bool ContainsString(IEnumerable<string> list, string value) =>
            list.Any(e => StringsAreSame(e, value));

        private bool StringsAreSame(string left, string right) =>
            string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;

        public string StoreTempCustomMap(IFormFile formFile)
        {
            string filename = Path.GetFileNameWithoutExtension(formFile.FileName);
            ValidateFilename(filename);

            string tempFolderName = Path.GetRandomFileName();
            string mapPath = Path.Combine(CommandLineUtils.TempMapDir, tempFolderName);
            if (!Directory.Exists(mapPath))
                Directory.CreateDirectory(mapPath);

            mapPath = Path.Combine(mapPath, filename + ".map");
            FileStream fs = new FileStream(mapPath, FileMode.CreateNew);
            formFile.CopyTo(fs);

            return tempFolderName;
        }

        private void ValidateFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new WebInterfaceException("Invalid filename");

            if (ContainsString(crypticMaps, filename + ".map"))
                throw new WebInterfaceException($"You cannot play this map ({filename}) as a custom map.");

            foreach (var chr in Path.GetInvalidFileNameChars())
            {
                if (filename.Contains(chr))
                    throw new WebInterfaceException("Invalid characters in the file name of the custom map.");
            }
        }
    }
}
