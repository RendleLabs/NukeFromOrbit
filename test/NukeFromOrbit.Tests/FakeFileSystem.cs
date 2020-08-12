using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using NSubstitute;

namespace NukeFromOrbit.Tests
{
    internal class FakeFileSystem : IFileSystem
    {
        public FakeFileSystem(IEnumerable<string> fileList)
        {
            var files = fileList.ToArray();
            var directories = AllDirectories(files).Distinct().ToArray();

            Path = new PathWrapper(this);
            File = Substitute.For<IFile>();
            Directory = Substitute.For<IDirectory>();
            Directory.EnumerateDirectories(Arg.Any<string>())
                .Returns(c => directories.Where(d => Path.GetDirectoryName(d) == c.Arg<string>()).Distinct());
            Directory.EnumerateFiles(Arg.Any<string>())
                .Returns(c => files.Where(f => Path.GetDirectoryName(f) == c.Arg<string>()).Distinct());
            Directory.Exists(Arg.Any<string>()).Returns(true);
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

        public IFile File { get; }
        public IDirectory Directory { get; }
        public IFileInfoFactory FileInfo { get; }
        public IFileStreamFactory FileStream { get; }
        public IPath Path { get; }
        public IDirectoryInfoFactory DirectoryInfo { get; }
        public IDriveInfoFactory DriveInfo { get; }
        public IFileSystemWatcherFactory FileSystemWatcher { get; }
    }
}