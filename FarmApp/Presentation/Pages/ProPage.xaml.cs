using FarmApp.Presentation.ViewModels;

namespace FarmApp.Presentation.Pages;

public partial class ProPage : ContentPage
{
    public ProPage(ProViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((ProViewModel)BindingContext).InicializarAsync();
    }
}
