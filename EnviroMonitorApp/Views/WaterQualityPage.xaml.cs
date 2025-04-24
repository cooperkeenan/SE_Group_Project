// Views/WaterQualityPage.xaml.cs
using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;

namespace EnviroMonitorApp.Views;

public partial class WaterQualityPage : ContentPage
{
    private readonly WaterQualityViewModel _vm;

    public WaterQualityPage(IEnvironmentalDataService svc)
    {
        InitializeComponent();
        _vm = new WaterQualityViewModel(svc);
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
