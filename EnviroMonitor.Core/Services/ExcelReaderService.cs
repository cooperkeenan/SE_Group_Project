using System.IO;
using OfficeOpenXml;

namespace EnviroMonitor.Core.Services
{
    public class ExcelReaderService
    {
        public ExcelReaderService()
        {
            // EPPlus requires setting the license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public ExcelPackage LoadPackage(string filePath)
            => new ExcelPackage(new FileInfo(filePath));
    }
}
