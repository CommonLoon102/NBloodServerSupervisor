using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        private List<string> ListableCustomMaps => Directory.GetFiles(Common.CommandLineUtils.BloodDir,
            "*.map", SearchOption.TopDirectoryOnly)
            .Select(m => Path.GetFileName(m))
            .Where(m => !ContainsString(crypticMaps, m))
            .ToList();

        public IList<string> ListCustomMaps() => ListableCustomMaps;

        public byte[] GetCustomMapBytes(string map)
        {
            if (ListableCustomMaps.Any(m => StringsAreSame(m, map)))
            {
                return File.ReadAllBytes(Path.Combine(Common.CommandLineUtils.BloodDir, map));
            }
            else
            {
                throw new Exception($"Cannot download this map: {map}");
            }
        }

        private bool ContainsString(IEnumerable<string> list, string value) =>
            list.Any(e => StringsAreSame(e, value));

        private bool StringsAreSame(string left, string right) =>
            string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;
    }
}
