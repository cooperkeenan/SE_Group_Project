using System.IO;
using OfficeOpenXml;

namespace EnviroMonitor.Core.Services
{
    /// <summary>
    /// Thin wrapper around EPPlus that sets the license context
    /// and provides a helper to open an <see cref="ExcelPackage"/>.
    /// </summary>
    public class ExcelReaderService
    {
        /// <summary>
        /// Constructor — sets EPPlus to the free
        /// <see cref="LicenseContext.NonCommercial"/> mode.
        /// </summary>
        public ExcelReaderService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Loads an <c>.xlsx</c> file into an <see cref="ExcelPackage"/>.
        /// </summary>
        /// <param name="filePath">Full path to the Excel workbook.</param>
        /// <returns>An open <see cref="ExcelPackage"/> ready for reading.</returns>
        public ExcelPackage LoadPackage(string filePath) =>
            new ExcelPackage(new FileInfo(filePath));
    }
}
