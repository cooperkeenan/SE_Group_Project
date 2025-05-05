using System.IO;
using EnviroMonitor.Core.Services;
using OfficeOpenXml;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    public class ExcelReaderServiceTests
    {
        [Fact]
        public void Constructor_SetsLicenseContext()
        {
            var svc = new ExcelReaderService();
            Assert.Equal(LicenseContext.NonCommercial, ExcelPackage.LicenseContext);
        }

        [Fact]
        public void LoadPackage_CanReadWrittenSheet()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".xlsx");

            // create a minimal package
            using (var pkg = new ExcelPackage(new FileInfo(tempFile)))
            {
                pkg.Workbook.Worksheets.Add("Sheet1");
                pkg.Save();
            }

            var svc = new ExcelReaderService();
            using var loaded = svc.LoadPackage(tempFile);

            Assert.NotNull(loaded.Workbook.Worksheets["Sheet1"]);
        }
    }
}
