using System.IO;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services
{
    /// <summary>
    /// Interface for file system operations that provides platform-independent access to app-specific directories 
    /// and files bundled with the application package.
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        /// Gets the platform-specific application data directory where the app can store files.
        /// </summary>
        string AppDataDirectory { get; }
        
        /// <summary>
        /// Opens a file that is bundled with the application package.
        /// </summary>
        /// <param name="filename">The name of the file to open.</param>
        /// <returns>A stream containing the file contents.</returns>
        Task<Stream> OpenAppPackageFileAsync(string filename);
    }
}