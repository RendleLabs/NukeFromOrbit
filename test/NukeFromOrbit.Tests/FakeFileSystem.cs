using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NSubstitute;

namespace NukeFromOrbit.Tests
{
    internal static class FakeFileSystem
    {
        public static IFileSystem Fake(IEnumerable<string> fileList)
        {
            var files = fileList.ToArray();
            var directories = AllDirectories(files).Distinct().ToArray();

            var fileSystem = Substitute.For<IFileSystem>();
            var path = new PathWrapper(fileSystem);
            var file = Substitute.For<IFile>();
            
            var directory = FakeDirectory(directories, path, files);

            fileSystem.Path.Returns(path);
            fileSystem.Directory.Returns(directory);
            fileSystem.File.Returns(file);
            return fileSystem;
        }

        private static IDirectory FakeDirectory(string[] directories, IPath path, string[] files)
        {
            var directory = Substitute.For<IDirectory>();
            
            directory.EnumerateDirectories(Arg.Any<string>())
                .Returns(c => directories.Where(d => path.GetDirectoryName(d) == c.Arg<string>()).Distinct());
            
            directory.EnumerateFiles(Arg.Any<string>())
                .Returns(c => files.Where(f => path.GetDirectoryName(f) == c.Arg<string>()).Distinct());
            
            directory.Exists(Arg.Any<string>()).Returns(true);
            
            return directory;
        }

        private static IEnumerable<string> AllDirectories(IEnumerable<string> source)
        {
            return source.SelectMany(AllDirectories);
        }

        private static IEnumerable<string> AllDirectories(string dir)
        {
            for (var d = System.IO.Path.GetDirectoryName(dir); !(d is null); d = System.IO.Path.GetDirectoryName(d))
            {
                yield return d;
            }
        }
    }
}