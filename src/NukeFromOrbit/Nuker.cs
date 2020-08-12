using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
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
        private readonly StringComparer _stringComparer;

        private Nuker(string workingDirectory, IFileSystem fileSystem, HashSet<string> gitFiles, IConsole console)
        {
            _workingDirectory = workingDirectory;
            _fileSystem = fileSystem;
            _gitFiles = gitFiles;
            _console = console;
            var isCaseSensitive = FileSystemUtil.IsCaseSensitive(workingDirectory);
            _stringComparison = isCaseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase;
            _stringComparer = isCaseSensitive ? StringComparer.CurrentCulture : StringComparer.CurrentCultureIgnoreCase;
        }
        
        public static async Task<Nuker> CreateAsync(string workingDirectory, IFileSystem? fileSystem = null, IGitFileList? gitFileList = null, IConsole? console = null)
        {
            fileSystem ??= new FileSystem();
            gitFileList ??= new GitFileList(workingDirectory);
            console ??= new DefaultConsole();
            var gitFiles = await gitFileList.GetAsync();
            return new Nuker(workingDirectory, fileSystem, gitFiles, console);
        }
        
        public IReadOnlyCollection<DeleteItem> GetItemsToBeNuked()
        {
            var entries = new Dictionary<string, ItemType>(_stringComparer);
            NukeDirectory(_workingDirectory, entries);
            return entries.Select(pair => new DeleteItem(pair.Key, pair.Value)).ToList().AsReadOnly();
        }

        private void NukeDirectory(string currentDirectory, Dictionary<string, ItemType> entries)
        {
            foreach (var directory in _fileSystem.Directory.EnumerateDirectories(currentDirectory))
            {
                var name = Path.GetFileName(directory);
                if (name is null) continue;

                if (name.Equals("bin", _stringComparison) || name.Equals("obj", _stringComparison))
                {
                    if (!_gitFiles.Contains(directory))
                    {
                        entries[directory] = ItemType.Directory;
                    }
                    else
                    {
                        NukeCarefully(directory, entries);
                    }
                }
                else
                {
                    NukeDirectory(directory, entries);
                }
            }
        }

        private void NukeCarefully(string directory, Dictionary<string, ItemType> entries)
        {
            foreach (var file in Directory.EnumerateFiles(directory))
            {
                if (!_gitFiles.Contains(file))
                {
                    entries[file] = ItemType.File;
                }
            }

            foreach (var subDirectory in Directory.EnumerateDirectories(directory))
            {
                NukeCarefully(subDirectory, entries);
            }
        }

        public void NukeItems(IEnumerable<DeleteItem> items)
        {
            foreach (var item in items)
            {
                if (item.Type == ItemType.Directory)
                {
                    try
                    {
                        _fileSystem.Directory.Delete(item.Path, true);
                        OutputDeleted(item.Path);
                    }
                    catch (Exception ex)
                    {
                        OutputDeleteError(item.Path, ex.Message);
                    }
                }
                else
                {
                    try
                    {
                        _fileSystem.File.Delete(item.Path);
                        OutputDeleted(item.Path);
                    }
                    catch (Exception ex)
                    {
                        OutputDeleteError(item.Path, ex.Message);
                    }
                }
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