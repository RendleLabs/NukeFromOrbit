using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NukeFromOrbit
{
    public static class FileSystemUtil
    {
        public static bool IsCaseSensitive(string directory)
        {
            if (!Directory.Exists(directory)) throw new InvalidOperationException("Directory does not exist.");
            
            if (directory.Where(char.IsLetter).Any(char.IsLower))
            {
                if (Directory.Exists(directory.ToUpper(CultureInfo.CurrentCulture))) return false;
            }
            else
            {
                if (Directory.Exists(directory.ToLower(CultureInfo.CurrentCulture))) return false;
            }

            return true;
        }
    }
}