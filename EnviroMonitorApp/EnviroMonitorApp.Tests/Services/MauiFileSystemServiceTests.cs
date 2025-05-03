// Services/MauiFileSystemServiceTests.cs
using System.IO;
using System.Threading.Tasks;
using EnviroMonitorApp.Services;
using Xunit;

namespace EnviroMonitorApp.Tests.Services;

public class MauiFileSystemServiceTests
{
    [Fact]
    public async Task OpenAppPackageFileAsync_ReturnsReadableStream()
    {
        // arrange: drop a dummy file into a temp folder
        var tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempRoot);
        var dummyPath = Path.Combine(tempRoot, "dummy.txt");
        await File.WriteAllTextAsync(dummyPath, "hello");

        var fs = new MauiFileSystemService(tempRoot);

        // act
        using var stream = await fs.OpenAppPackageFileAsync("dummy.txt");

        // assert
        Assert.True(stream.CanRead);
        Assert.Equal(5, stream.Length);

        Directory.Delete(tempRoot, true);
    }
}
