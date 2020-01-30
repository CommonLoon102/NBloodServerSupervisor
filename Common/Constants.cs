using Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class Constants
    {
        public const long FileSizeLimit = 1024 * 1024;

        public static readonly IReadOnlyDictionary<string, Mod> SupportedMods = new Dictionary<string, Mod>()
        {
            { "BLOOD", new Mod("BLOOD", "Blood", "") },
            { "CRYPTIC", new Mod("CRYPTIC", "Cryptic Passage", "-ini CRYPTIC.INI") },
            { "DW", new Mod("DW", "Death Wish", "-ini dw.ini") },
            { "TWOIRA", new Mod("TWOIRA", "The Way of Ira", "-ini TWOIRA/twoira.ini -j=TWOIRA") },
        };
    }
}
