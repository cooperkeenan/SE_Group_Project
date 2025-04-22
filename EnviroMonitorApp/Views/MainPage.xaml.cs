using EnviroMonitorApp.Services;
using System.Diagnostics;

namespace EnviroMonitorApp.Views;

public partial class MainPage : ContentPage
{
    private readonly ExcelReaderService _excel;

    public MainPage(ExcelReaderService excel)
    {
        Debug.WriteLine("🔥 MainPage constructor fired");
        InitializeComponent();
        _excel = excel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine("📍 OnAppearing triggered");

        StatusLabel.Text = "🔄 Reading...";

        try
        {
            var data = _excel.ReadWeather("Weather.xlsx");
            Debug.WriteLine($"📦 Data rows: {data.Count}");

            if (data.Any())
            {
                Debug.WriteLine($"🔥 First row temp: {data[0].Temperature}");
                StatusLabel.Text = $"✅ {data.Count} rows loaded";
            }
            else
            {
                StatusLabel.Text = "⚠️ No data found";
            }

            await DisplayAlert("✅ Success", $"Loaded {data.Count} records", "OK");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Excel parsing failed: {ex}");
            StatusLabel.Text = $"❌ Error: {ex.Message}";
            await DisplayAlert("❌ Error", ex.Message, "OK");
        }
    }
}
