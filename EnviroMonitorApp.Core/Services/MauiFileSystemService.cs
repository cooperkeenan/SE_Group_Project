// EnviroMonitorApp.Core/Services/MauiFileSystemService.cs
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace EnviroMonitorApp.Services
{
    public class MauiFileSystemService : IFileSystemService
    {
        public string AppDataDirectory => FileSystem.AppDataDirectory;

        public Task<Stream> OpenAppPackageFileAsync(string filename)
        {
            return FileSystem.OpenAppPackageFileAsync(filename);
        }
    }
}