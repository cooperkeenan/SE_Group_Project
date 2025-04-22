using EnviroMonitorApp.Services;
using EnviroMonitorApp.Models;
using System.Diagnostics;

namespace EnviroMonitorApp.Views;

public partial class MainPage : ContentPage
{
    private readonly ExcelReaderService _excel;

    public MainPage(ExcelReaderService excel)
    {
        InitializeComponent();
        _excel = excel;
    }

    protected override async void OnAppearing()
		{
			base.OnAppearing();
			Debug.WriteLine("OnAppearing triggered");

			StatusLabel.Text = "Reading files...";

			try
			{
				// ✅ Corrected parser usage
				var weatherData = _excel.ReadWeather("Weather.xlsx");

				var airData = _excel.ReadAirQuality("Air_quality.xlsx");
				Debug.WriteLine($"Air rows: {airData.Count}");
				Debug.WriteLine($"First PM2.5: {airData[0].PM25}");

				var waterData = _excel.ReadWaterQuality("Water_quality.xlsx");
				Debug.WriteLine($"Water rows: {waterData.Count}");
				Debug.WriteLine($"First EC: {waterData[0].EC}");

				StatusLabel.Text = $"Loaded {weatherData.Count} weather, {airData.Count} air, {waterData.Count} water rows.";

				DataList.ItemsSource = weatherData;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Excel parsing failed: {ex}");
				StatusLabel.Text = $"Error: {ex.Message}";
				await DisplayAlert("Error", ex.Message, "OK");
			}
		}

}
