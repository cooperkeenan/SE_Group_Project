using System.Diagnostics;
using EnviroMonitorApp.Models;
using OfficeOpenXml;

namespace EnviroMonitorApp.Services
{
    public class ExcelReaderService
    {
        public ExcelReaderService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        }

        public List<WeatherRecord> ReadWeather(string filename)
        {
            var results = new List<WeatherRecord>();

            var file = FileSystem.OpenAppPackageFileAsync(filename).Result;

            using var package = new ExcelPackage(file);
            var sheet = package.Workbook.Worksheets[0];

            for (int row = 5; sheet.Cells[row, 1].Value != null; row++)
            {
                try
                {
                    results.Add(new WeatherRecord
                    {
                        DateTime = Convert.ToDateTime(sheet.Cells[row, 1].Text),
                        Temperature = float.Parse(sheet.Cells[row, 2].Text),
                        RelativeHumidity = int.Parse(sheet.Cells[row, 3].Text),
                        WindSpeed = float.Parse(sheet.Cells[row, 4].Text),
                        WindDirection = int.Parse(sheet.Cells[row, 5].Text)
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Weather] Error parsing row {row}: {ex.Message}");
                }
            }

            return results;
        }

        public List<AirQualityRecord> ReadAirQuality(string filename)
        {
            var results = new List<AirQualityRecord>();

            var file = FileSystem.OpenAppPackageFileAsync(filename).Result;

            using var package = new ExcelPackage(file);
            var sheet = package.Workbook.Worksheets[0];

            for (int row = 11; sheet.Cells[row, 1].Value != null; row++)
            {
                try
                {
                    var date = DateTime.Parse(sheet.Cells[row, 1].Text);
                    var time = TimeSpan.Parse(sheet.Cells[row, 2].Text);
                    var dateTime = date + time;

                    results.Add(new AirQualityRecord
                    {
                        DateTime = dateTime,
                        NitrogenDioxide = double.Parse(sheet.Cells[row, 3].Text),
                        SulphurDioxide = double.Parse(sheet.Cells[row, 4].Text),
                        PM25 = double.Parse(sheet.Cells[row, 5].Text),
                        PM10 = double.Parse(sheet.Cells[row, 6].Text)
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Air] Error parsing row {row}: {ex.Message}");
                }
            }

            return results;
        }

        public List<WaterQualityRecord> ReadWaterQuality(string filename)
        {
            var results = new List<WaterQualityRecord>();

            var file = FileSystem.OpenAppPackageFileAsync(filename).Result;

            using var package = new ExcelPackage(file);
            var sheet = package.Workbook.Worksheets[0];

            for (int row = 6; sheet.Cells[row, 1].Value != null; row++)
            {
                try
                {
                    var date = DateTime.Parse(sheet.Cells[row, 1].Text);
                    var time = TimeSpan.Parse(sheet.Cells[row, 2].Text);
                    var dateTime = date + time;

                    results.Add(new WaterQualityRecord
                    {
                        DateTime = dateTime,
                        Nitrate = double.Parse(sheet.Cells[row, 3].Text),
                        Nitrite = double.Parse(sheet.Cells[row, 4].Text),
                        Phosphate = double.Parse(sheet.Cells[row, 5].Text),
                        EC = double.Parse(sheet.Cells[row, 6].Text)
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Water] Error parsing row {row}: {ex.Message}");
                }
            }

            return results;
        }
    }
}
