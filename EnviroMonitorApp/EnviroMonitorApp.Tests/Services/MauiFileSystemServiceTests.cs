using System.IO;
using System.Threading.Tasks;
using EnviroMonitorApp.Services;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests.Services;

public class MauiFileSystemServiceTests
{
    [Fact]
    public async Task OpenAppPackageFileAsync_Reads_Embedded_DB()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempRoot);
        var dummyPath = Path.Combine(tempRoot, "dummy.txt");
        await File.WriteAllTextAsync(dummyPath, "hello");

        // Mocking IFileSystemService
        var fs = new Mock<IFileSystemService>();
        fs.Setup(f => f.AppDataDirectory).Returns(tempRoot); // mocking read-only property

        await using var stream = await fs.Object.OpenAppPackageFileAsync("dummy.txt");

        Assert.True(stream.CanRead);
        Assert.Equal(5, stream.Length);

        Directory.Delete(tempRoot, true);
    }
}
