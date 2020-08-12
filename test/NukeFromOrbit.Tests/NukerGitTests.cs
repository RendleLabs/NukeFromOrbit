using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace NukeFromOrbit.Tests
{
    public class NukerGitTests
    {
        private static readonly string[] Files =
        {
            @"D:\Fake\src\Fake\Fake.csproj",
            @"D:\Fake\src\Fake\Fake.cs",
            @"D:\Fake\src\Fake\bin\Debug\Fake.dll",
            @"D:\Fake\src\Fake\bin\IAmVersionControlled.txt",
            @"D:\Fake\src\Fake\obj\project.assets.json",
        };
        
        [Fact]
        public async Task GetsItemsToBeNuked()
        {
            var fakeFileSystem = new FakeFileSystem(Files);
            
            var gitFileList = FakeFileList(fakeFileSystem);

            var fakeConsole = Substitute.For<IConsole>();
            
            var nuker = await Nuker.CreateAsync(@"D:\Fake", fakeFileSystem, gitFileList, fakeConsole);

            var actual = nuker.GetItemsToBeNuked();
            Assert.Contains(actual, i => i.Path == @"D:\Fake\src\Fake\bin\Debug\Fake.dll" && i.Type == ItemType.File);
            Assert.Contains(actual, i => i.Path == @"D:\Fake\src\Fake\obj" && i.Type == ItemType.Directory);
            Assert.DoesNotContain(actual, i => i.Path == @"D:\Fake\src\Fake\bin\IAmVersionControlled.txt");
        }

        private static GitFileList FakeFileList(FakeFileSystem fakeFileSystem)
        {
            var gitFiles = new HashSet<string>(Files, StringComparer.OrdinalIgnoreCase);
            gitFiles.Remove(@"D:\Fake\src\Fake\bin\Debug\Fake.dll");
            gitFiles.Remove(@"D:\Fake\src\Fake\obj\project.assets.json");

            var gitFileList = new GitFileList(@"D:\Fake", fakeFileSystem, () => Task.FromResult(gitFiles));
            return gitFileList;
        }

        [Fact]
        public async Task NukesItems()
        {
            var fakeFileSystem = new FakeFileSystem(Files);
            var gitFileList = FakeFileList(fakeFileSystem);
            var fakeConsole = Substitute.For<IConsole>();
            
            var nuker = await Nuker.CreateAsync(@"D:\Fake", fakeFileSystem, gitFileList, fakeConsole);

            var actual = nuker.GetItemsToBeNuked();
            nuker.NukeItems(actual);

            fakeFileSystem.Directory.DidNotReceive().Delete(@"D:\Fake\src\Fake\bin", true);
            fakeFileSystem.File.Received().Delete(@"D:\Fake\src\Fake\bin\Debug\Fake.dll");
            fakeFileSystem.Directory.Received().Delete(@"D:\Fake\src\Fake\obj", true);
        }
    }
}