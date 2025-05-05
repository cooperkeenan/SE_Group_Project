// EnviroMonitorApp.Core/Services/MauiFileSystemService.cs
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Implementation of IFileSystemService that uses MAUI's FileSystem APIs
    /// to provide cross-platform file system access.
    /// </summary>
    public class MauiFileSystemService : IFileSystemService
    {
        /// <summary>
        /// Gets the platform-specific application data directory where the app can store files.
        /// This directory is typically private to the application.
        /// </summary>
        public string AppDataDirectory => FileSystem.AppDataDirectory;

        /// <summary>
        /// Opens a file that is bundled with the application package.
        /// </summary>
        /// <param name="filename">The name of the file to open.</param>
        /// <returns>A stream containing the file contents.</returns>
        public Task<Stream> OpenAppPackageFileAsync(string filename)
        {
            return FileSystem.OpenAppPackageFileAsync(filename);
        }
    }
}