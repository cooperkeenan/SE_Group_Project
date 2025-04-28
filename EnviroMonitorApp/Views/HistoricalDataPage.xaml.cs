using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls;
using EnviroMonitorApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Android.Util;  // for Log.Error

namespace EnviroMonitorApp.Views
{
    public partial class HistoricalDataPage : ContentPage
    {
        readonly HistoricalDataViewModel _vm;

        // Shell will use this parameterless ctor
        public HistoricalDataPage()
            : this(App.Current!
                    .Handler.MauiContext
                    .Services
                    .GetRequiredService<HistoricalDataViewModel>())
        {
            Debug.WriteLine("⚙️ HistoricalDataPage(): parameterless ctor called");
        }

        // DI-friendly ctor
        public HistoricalDataPage(HistoricalDataViewModel vm)
        {
            Debug.WriteLine("⚙️ HistoricalDataPage(vm): DI ctor called");
            _vm = vm;

            try
            {
                InitializeComponent();
                Debug.WriteLine("⚙️ HistoricalDataPage: InitializeComponent complete");
            }
            catch (Exception ex)
            {
                // Log XAML‐inflation errors so we can see them in logcat
                Log.Error("XAML_ERROR", $"HistoricalDataPage InitializeComponent error: {ex}");
                throw;
            }

            BindingContext = _vm;
            Debug.WriteLine("⚙️ HistoricalDataPage: BindingContext set");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("⚙️ HistoricalDataPage: OnAppearing");

            try
            {
                if (!_vm.ChartData.Any())
                {
                    Debug.WriteLine("⚙️ HistoricalDataPage: firing LoadDataCommand");
                    _vm.LoadDataCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ HistoricalDataPage.OnAppearing error: {ex}");
                DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
