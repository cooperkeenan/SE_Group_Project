// Views/HistoricalDataPage.xaml.cs
using System.Diagnostics;
using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace EnviroMonitorApp.Views
{
    public partial class HistoricalDataPage : ContentPage
    {
        readonly HistoricalDataViewModel _vm;

        public HistoricalDataPage()
            : this(App.Current!.Handler.MauiContext.Services.GetRequiredService<HistoricalDataViewModel>())
        {
            Debug.WriteLine("⚙️ HistoricalDataPage(): parameterless ctor called");
        }

        public HistoricalDataPage(HistoricalDataViewModel vm)
        {
            Debug.WriteLine("⚙️ HistoricalDataPage(vm): DI ctor called");
            InitializeComponent();
            BindingContext = _vm = vm;
            Debug.WriteLine("⚙️ HistoricalDataPage: BindingContext set");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Debug.WriteLine("⚙️ HistoricalDataPage: OnAppearing");

            if (_vm.ChartData.Count == 0 && _vm.LoadDataCommand.CanExecute(null))
            {
                Debug.WriteLine("⚙️ HistoricalDataPage: executing LoadDataCommand");
                _vm.LoadDataCommand.Execute(null);
            }
        }
    }
}
