﻿using System;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace NukeFromOrbit
{
    public static class FileSystemUtil
    {
        public static bool IsCaseSensitive(IFileSystem fileSystem, string directory)
        {
            if (!fileSystem.Directory.Exists(directory)) throw new InvalidOperationException("Directory does not exist.");
            
            if (directory.Where(char.IsLetter).Any(char.IsLower))
            {
                if (fileSystem.Directory.Exists(directory.ToUpper(CultureInfo.CurrentCulture))) return false;
            }
            else
            {
                if (fileSystem.Directory.Exists(directory.ToLower(CultureInfo.CurrentCulture))) return false;
            }

            return true;
        }
    }
}