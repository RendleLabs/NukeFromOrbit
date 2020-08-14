using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace NukeFromOrbit.Tests
{
    public class NukerTests
    {
        private static readonly string[] Files =
        {
            @"D:\Fake\src\Fake\Fake.csproj",
            @"D:\Fake\src\Fake\Fake.cs",
            @"D:\Fake\src\Fake\bin\Debug\Fake.dll",
            @"D:\Fake\src\Fake\obj\project.assets.json",
        };
        
        [Fact]
        public async Task GetsItemsToBeNuked()
        {
            var fakeFileSystem = FakeFileSystem.Fake(Files);
            var fakeGitFileList = new FakeGitFileList();
            var fakeConsole = Substitute.For<IConsole>();
            
            var nuker = await Nuker.CreateAsync(@"D:\Fake", fakeFileSystem, fakeGitFileList, fakeConsole);

            var actual = nuker.GetItemsToBeNuked();
            Assert.Contains(actual, i => i.Path == @"D:\Fake\src\Fake\bin" && i.Type == ItemType.Directory);
            Assert.Contains(actual, i => i.Path == @"D:\Fake\src\Fake\obj" && i.Type == ItemType.Directory);
        }
        
        [Fact]
        public async Task NukesItems()
        {
            var fakeFileSystem = FakeFileSystem.Fake(Files);
            var fakeGitFileList = new FakeGitFileList();
            var fakeConsole = Substitute.For<IConsole>();
            
            var nuker = await Nuker.CreateAsync(@"D:\Fake", fakeFileSystem, fakeGitFileList, fakeConsole);

            var actual = nuker.GetItemsToBeNuked();
            nuker.NukeItems(actual);

            fakeFileSystem.Directory.Received().Delete(@"D:\Fake\src\Fake\bin", true);
            fakeFileSystem.Directory.Received().Delete(@"D:\Fake\src\Fake\obj", true);
        }
    }
}