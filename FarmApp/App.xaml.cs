namespace FarmApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        // Seguir el tema del sistema (claro u oscuro según el dispositivo)
        UserAppTheme = AppTheme.Unspecified;

        MainPage = new AppShell();
    }
}
