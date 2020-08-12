using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CliWrap;

namespace NukeFromOrbit
{
    public class GitFileList : IGitFileList
    {
        private static readonly string Bin = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
        private static readonly string Obj = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";
        private readonly string _workingDirectory;
        private readonly StringComparison _stringComparison;
        private readonly StringComparer _stringComparer;

        public GitFileList(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            var isCaseSensitiveFileSystem = FileSystemUtil.IsCaseSensitive(workingDirectory);
            _stringComparison = isCaseSensitiveFileSystem ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            _stringComparer = isCaseSensitiveFileSystem ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
        }
        
        public async Task<HashSet<string>> GetAsync()
        {
            var set = new HashSet<string>(_stringComparer);

            var result = await Cli.Wrap("git")
                .WithArguments("ls-files")
                .WithWorkingDirectory(_workingDirectory)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
                {
                    if (Path.DirectorySeparatorChar != '/')
                    {
                        line = line.Replace('/', Path.DirectorySeparatorChar);
                    }
                    if (line.Contains(Bin) || line.Contains(Obj))
                    {
                        set.Add(Path.Combine(_workingDirectory, line));
                    }
                }))
                .ExecuteAsync();

            if (result.ExitCode != 0)
            {
                set.Clear();
                return set;
            }

            if (set.Count == 0) return set;

            foreach (var line in set.ToArray())
            {
                for (var parent = Path.GetDirectoryName(line); InWorkingDirectory(parent); parent = Path.GetDirectoryName(parent))
                {
                    set.Add(parent);
                }
            }
            
            return set;
        }

        private bool InWorkingDirectory(string? directory)
        {
            if (directory is null) return false;
            if (directory.Equals(_workingDirectory, _stringComparison)) return false;
            return directory.StartsWith(_workingDirectory, _stringComparison);
        }

    }

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