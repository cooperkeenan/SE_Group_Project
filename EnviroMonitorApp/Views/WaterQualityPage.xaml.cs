using EnviroMonitorApp.Services;
using EnviroMonitorApp.ViewModels;

namespace EnviroMonitorApp.Views;

public partial class WaterQualityPage : ContentPage
{
    public WaterQualityPage(IEnvironmentalDataService service)
    {
        InitializeComponent();
        BindingContext = new WaterQualityViewModel(service);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await ((WaterQualityViewModel)BindingContext).LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Water error", ex.Message, "OK");
        }
    }
}
