using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    [Serializable]
    public class Mod
    {
        public string Name { get; }
        public string FriendlyName { get; }
        public string CommandLine { get; }

        public Mod(string name, string friendlyName, string cmdLine)
        {
            Name = name;
            FriendlyName = friendlyName;
            CommandLine = cmdLine;
        }
    }
}
