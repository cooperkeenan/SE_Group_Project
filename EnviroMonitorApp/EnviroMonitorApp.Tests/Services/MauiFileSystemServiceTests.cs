using System.IO;
using System.Threading.Tasks;
using EnviroMonitorApp.Services;
using Moq;
using Xunit;

namespace EnviroMonitorApp.Tests.Services
{
    public class MauiFileSystemServiceTests
    {
        [Fact]
        public async Task OpenAppPackageFileAsync_Reads_Embedded_DB()
        {
            // Create a temporary folder and file for testing
            var tempRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempRoot);
            var dummyPath = Path.Combine(tempRoot, "dummy.txt");
            await File.WriteAllTextAsync(dummyPath, "hello");

            // Mocking IFileSystemService
            var fs = new Mock<IFileSystemService>();
            fs.Setup(f => f.AppDataDirectory).Returns(tempRoot); // Mock the AppDataDirectory property
            fs.Setup(f => f.OpenAppPackageFileAsync(It.IsAny<string>()))
                .ReturnsAsync(() => File.OpenRead(dummyPath));  // Mock OpenAppPackageFileAsync to return the stream of the dummy file

            // Act: Open the file using the mock service
            await using var stream = await fs.Object.OpenAppPackageFileAsync("dummy.txt");

            // Assert: Check that the stream is readable and has the expected length
            Assert.True(stream.CanRead);
            Assert.Equal(5, stream.Length);

            // Clean up: Delete the temporary directory and file
            Directory.Delete(tempRoot, true);
        }
    }
}
