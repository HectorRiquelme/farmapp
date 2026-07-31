using FarmApp.Constants;
using FarmApp.Domain.Interfaces;

namespace FarmApp;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(IProService proService)
    {
        InitializeComponent();

        AplicarTemaGuardado();

        // Detecta reembolsos de FarmApp Pro en segundo plano (nunca lanza excepciones)
        _ = proService.RevalidarSilenciosoAsync();
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new AppShell());

    /// <summary>
    /// Aplica el tema elegido por el usuario Pro; por defecto sigue al sistema.
    /// </summary>
    private void AplicarTemaGuardado()
    {
        var tema = Preferences.Default.Get(AppConstants.PrefTemaApp, "sistema");

        UserAppTheme = tema switch
        {
            "claro" => AppTheme.Light,
            "oscuro" => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };
    }
}
