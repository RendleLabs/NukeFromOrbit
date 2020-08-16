using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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
        private readonly IFileSystem _fileSystem;
        private readonly StringComparison _stringComparison;
        private readonly StringComparer _stringComparer;
        private readonly Func<Task<HashSet<string>>> _listFiles;

        public GitFileList(string workingDirectory, IFileSystem? fileSystem = null, Func<Task<HashSet<string>>>? listFiles = null)
        {
            _workingDirectory = workingDirectory;
            _fileSystem = fileSystem ?? new FileSystem();
            _listFiles = listFiles ?? ListFileAsync;
            
            var isCaseSensitiveFileSystem = _fileSystem.IsCaseSensitive(workingDirectory);
            _stringComparison = isCaseSensitiveFileSystem ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            _stringComparer = isCaseSensitiveFileSystem ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
        }
        
        public async Task<HashSet<string>> GetAsync()
        {
            var set = await _listFiles();

            if (set.Count == 0) return set;

            foreach (var line in set.ToArray())
            {
                for (var parent = _fileSystem.Path.GetDirectoryName(line); InWorkingDirectory(parent); parent = _fileSystem.Path.GetDirectoryName(parent))
                {
                    set.Add(parent!);
                }
            }
            
            return set;
        }

        private async Task<HashSet<string>> ListFileAsync()
        {
            var set = new HashSet<string>(_stringComparer);
            
            var result = await Cli.Wrap("git")
                .WithArguments("ls-files")
                .WithWorkingDirectory(_workingDirectory)
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
                {
                    line = _fileSystem.Path.Normalize(line);
                    
                    if (line.Contains(Bin) || line.Contains(Obj))
                    {
                        set.Add(Path.Combine(_workingDirectory, line));
                    }
                }))
                .ExecuteAsync();

            if (result.ExitCode != 0)
            {
                set.Clear();
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
}