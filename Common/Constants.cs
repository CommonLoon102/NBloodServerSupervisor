using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common
{
    public class Constants
    {
        public const long FileSizeLimit = 1024 * 1024;

        private const string workingDirectory = "WD";
        private static readonly string iraArt = Path.Combine(workingDirectory, "IRA");
        private static readonly string iraRff = Path.Combine(workingDirectory, "BLOOD.RFF");
        private static readonly string iraSound = Path.Combine(workingDirectory, "SOUNDS.RFF");

        public static readonly IReadOnlyDictionary<string, Mod> SupportedMods = new Dictionary<string, Mod>()
        {
            { "BLOOD", new Mod("BLOOD", "Blood", "") },
            { "CRYPTIC", new Mod("CRYPTIC", "Cryptic Passage", "-ini CRYPTIC.INI") },
            { "DW", new Mod("DW", "Death Wish", "-ini dw.ini") },
            { "TWOIRA", new Mod("TWOIRA", "The Way of Ira", $"-game_dir IRA -ini ira.ini -art {iraArt} -rff {iraRff} -snd {iraSound}") },
        };
    }
}
