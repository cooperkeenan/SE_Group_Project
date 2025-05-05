using System.IO;
using EnviroMonitor.Core.Services;
using OfficeOpenXml;
using Xunit;

namespace EnviroMonitorApp.Tests
{
    /// <summary>
    /// Covers basic behaviour of <see cref="ExcelReaderService"/>.
    /// </summary>
    public class ExcelReaderServiceTests
    {
        /// <summary>
        /// Constructor should switch EPPlus into the free
        /// <see cref="LicenseContext.NonCommercial"/> mode.
        /// </summary>
        [Fact]
        public void Constructor_SetsLicenseContext()
        {
            var svc = new ExcelReaderService();

            Assert.Equal(
                LicenseContext.NonCommercial,
                ExcelPackage.LicenseContext);
        }

        /// <summary>
        /// Verifies that the helper can load a file created on disk and
        /// that a worksheet written earlier is still present.
        /// </summary>
        [Fact]
        public void LoadPackage_CanReadWrittenSheet()
        {
            // create a temporary workbook with a single sheet
            var tempFile = Path.Combine(
                Path.GetTempPath(),
                Path.GetRandomFileName() + ".xlsx");

            using (var pkg = new ExcelPackage(new FileInfo(tempFile)))
            {
                pkg.Workbook.Worksheets.Add("Sheet1");
                pkg.Save();
            }

            // act
            var svc    = new ExcelReaderService();
            using var loaded = svc.LoadPackage(tempFile);

            // assert
            Assert.NotNull(loaded.Workbook.Worksheets["Sheet1"]);
        }
    }
}
