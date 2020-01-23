using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Supervisor
{
    static class StringUtils
    {
        public static string[] SplitMessage(this string message)
        {
            return message.Substring(1)
                .Split('\t', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim('\0'))
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();
        }
    }
}
