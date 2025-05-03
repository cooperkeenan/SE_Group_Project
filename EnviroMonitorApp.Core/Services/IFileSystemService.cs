using System.IO;
using System.Threading.Tasks;

namespace EnviroMonitorApp.Services
{
    public interface IFileSystemService
    {
        string AppDataDirectory { get; }
        Task<Stream> OpenAppPackageFileAsync(string filename);
    }
}
