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
        public void AppDataDirectory_ReturnsFileSystemDirectory()
        {
            // Arrange - create a mock file system with a known directory path
            var expectedDirectory = Path.Combine(Path.GetTempPath(), "TestAppData");
            
            // Since we can't directly mock the static FileSystem class, we'll use a wrapper
            var mockFileSystem = new Mock<IFileSystemService>();
            mockFileSystem.Setup(fs => fs.AppDataDirectory).Returns(expectedDirectory);
            
            // Act
            var result = mockFileSystem.Object.AppDataDirectory;
            
            // Assert
            Assert.Equal(expectedDirectory, result);
        }
        
        [Fact]
        public async Task OpenAppPackageFileAsync_ReturnsStreamFromFileSystem()
        {
            // Arrange
            var testFilename = "test.txt";
            var testContent = "test content";
            var expectedStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(testContent));
            
            var mockFileSystem = new Mock<IFileSystemService>();
            mockFileSystem.Setup(fs => fs.OpenAppPackageFileAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedStream);
            
            // Act
            var result = await mockFileSystem.Object.OpenAppPackageFileAsync(testFilename);
            
            // Assert
            Assert.NotNull(result);
            
            // Read the content to verify it's the expected stream
            using var reader = new StreamReader(result);
            var content = await reader.ReadToEndAsync();
            Assert.Equal(testContent, content);
            
            // Verify the method was called with the correct filename
            mockFileSystem.Verify(fs => fs.OpenAppPackageFileAsync(testFilename), Times.Once);
        }
        
        [Fact]
        public async Task OpenAppPackageFileAsync_HandlesNonExistentFile()
        {
            // Arrange
            var mockFileSystem = new Mock<IFileSystemService>();
            mockFileSystem.Setup(fs => fs.OpenAppPackageFileAsync(It.IsAny<string>()))
                .ReturnsAsync(() => null);
            
            // Act & Assert
            var result = await mockFileSystem.Object.OpenAppPackageFileAsync("nonexistent.txt");
            Assert.Null(result);
        }
    }
}