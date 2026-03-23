using FarmApp.Presentation.ViewModels;

namespace FarmApp.Presentation.Pages;

public partial class DetallePage : ContentPage
{
    public DetallePage(DetalleFarmaciaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
