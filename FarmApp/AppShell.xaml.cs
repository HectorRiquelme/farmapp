using FarmApp.Presentation.Pages;

namespace FarmApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registrar rutas de navegación para Shell.GoToAsync
        Routing.RegisterRoute(nameof(ResultadosPage), typeof(ResultadosPage));
        Routing.RegisterRoute(nameof(DetallePage), typeof(DetallePage));
    }
}
