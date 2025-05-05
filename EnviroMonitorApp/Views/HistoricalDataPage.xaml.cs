// Views/HistoricalDataPage.xaml.cs
using System.Diagnostics;
using EnviroMonitorApp.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace EnviroMonitorApp.Views
{
    /// <summary>
    /// Page for displaying and interacting with historical environmental data.
    /// Allows users to select data types, date ranges, and view visualizations.
    /// </summary>
    public partial class HistoricalDataPage : ContentPage
    {
        readonly HistoricalDataViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the HistoricalDataPage class.
        /// Uses dependency injection to get the required ViewModel instance.
        /// </summary>
        public HistoricalDataPage()
            : this(App.Current!.Handler.MauiContext.Services.GetRequiredService<HistoricalDataViewModel>())
        {
            Debug.WriteLine("⚙️ HistoricalDataPage(): parameterless ctor called");
        }

        /// <summary>
        /// Initializes a new instance of the HistoricalDataPage class with a specific ViewModel.
        /// This constructor is used for dependency injection and testing.
        /// </summary>
        /// <param name="vm">The ViewModel to use with this page.</param>
        public HistoricalDataPage(HistoricalDataViewModel vm)
        {
            Debug.WriteLine("⚙️ HistoricalDataPage(vm): DI ctor called");
            InitializeComponent();
            BindingContext = _vm = vm;
            Debug.WriteLine("⚙️ HistoricalDataPage: BindingContext set");
        }

        /// <summary>
        /// Called when the page appears.
        /// Loads data if the chart is empty and data loading is possible.
        /// </summary>
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