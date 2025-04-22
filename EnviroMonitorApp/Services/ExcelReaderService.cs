using EnviroMonitorApp.Models;
using ClosedXML.Excel;
using System.Diagnostics;

namespace EnviroMonitorApp.Services;

public class ExcelReaderService
{
    public List<WeatherRecord> ReadWeather(string filename)
    {
        var results = new List<WeatherRecord>();

        try
        {
            Debug.WriteLine($"📁 Trying to open: {filename}");

            var streamTask = FileSystem.OpenAppPackageFileAsync(filename);
            streamTask.Wait();

            using var stream = streamTask.Result;
            Debug.WriteLine("📂 Stream loaded ✅");

            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);
            Debug.WriteLine($"📄 Opened sheet: {sheet.Name}");

            // Skip metadata rows (1-4)
            foreach (var row in sheet.RowsUsed().Skip(4))
            {
                var record = new WeatherRecord
                {
                    Date = row.Cell(1).GetString(),
                    Temperature = row.Cell(2).GetDouble(),
                    Humidity = row.Cell(3).GetDouble()
                };

                Debug.WriteLine($"🌡️ Temp={record.Temperature}, 💧 RH={record.Humidity}");
                results.Add(record);
            }

            Debug.WriteLine($"✅ Finished reading {results.Count} rows.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ EXCEPTION in ReadWeather(): {ex}");
            throw;
        }

        return results;
    }
}
