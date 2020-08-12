using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace NukeFromOrbit
{
    public class Nuker
    {
        private readonly string _workingDirectory;
        private readonly IFileSystem _fileSystem;
        private readonly HashSet<string> _gitFiles;
        private readonly IConsole _console;
        private readonly StringComparison _stringComparison;

        private Nuker(string workingDirectory, IFileSystem fileSystem, HashSet<string> gitFiles, IConsole console)
        {
            _workingDirectory = workingDirectory;
            _fileSystem = fileSystem;
            _gitFiles = gitFiles;
            _console = console;
            var isCaseSensitive = FileSystemUtil.IsCaseSensitive(workingDirectory);
            _stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
        }
        
        public static async Task<Nuker> CreateAsync(string workingDirectory, IFileSystem fileSystem = null, IGitFileList gitFileList = null, IConsole console = null)
        {
            fileSystem ??= new FileSystem();
            gitFileList ??= new GitFileList(workingDirectory);
            console ??= new DefaultConsole();
            var gitFiles = await gitFileList.GetAsync();
            return new Nuker(workingDirectory, fileSystem, gitFiles, console);
        }

        public void Nuke()
        {
            NukeDirectory(_workingDirectory);
        }

        private void NukeDirectory(string currentDirectory)
        {
            foreach (var directory in _fileSystem.Directory.EnumerateDirectories(currentDirectory))
            {
                var name = Path.GetFileName(directory);
                if (name is null) continue;

                if (name.Equals("bin", _stringComparison) || name.Equals("obj", _stringComparison))
                {
                    if (!_gitFiles.Contains(directory))
                    {
                        try
                        {
                            _fileSystem.Directory.Delete(directory, true);
                        }
                        catch (Exception ex)
                        {
                            OutputDeleteError(directory, ex.Message);
                        }
                        OutputDeleted(directory);
                    }
                    else
                    {
                        NukeCarefully(directory);
                    }
                }
                else
                {
                    NukeDirectory(directory);
                }
            }
        }

        private void NukeCarefully(string directory)
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                if (!_gitFiles.Contains(file))
                {
                    try
                    {
                        _fileSystem.File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        OutputDeleteError(file, ex.Message);
                    }
                    OutputDeleted(file);
                }
            }

            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                NukeCarefully(subDirectory);
            }
        }

        private void OutputDeleted(string path)
        {
            path = Path.GetRelativePath(_workingDirectory, path);
            _console.WriteLine($"{path} deleted.");
        }

        private void OutputDeleteError(string path, string message)
        {
            path = Path.GetRelativePath(_workingDirectory, path);
            _console.WriteLine($"{path} could not be deleted: '{message}'.");
        }
    }
}