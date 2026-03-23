using FarmApp.Presentation.ViewModels;

namespace FarmApp.Presentation.Pages;

public partial class HomePage : ContentPage
{
    private bool _hasAnimated;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasAnimated) return;
        _hasAnimated = true;

        // ── Estado inicial: invisible y ligeramente desplazado ──
        LogoBranding.Opacity = 0;
        LogoBranding.Scale = 0.82;
        LogoBranding.TranslationY = 16;

        ActionSection.Opacity = 0;
        ActionSection.TranslationY = 28;

        // Pequeña pausa para que el layout esté listo
        await Task.Delay(80);

        // ── Fase 1: Logo aparece con spring (como si rebotara levemente) ──
        await Task.WhenAll(
            LogoBranding.FadeTo(1, 520, Easing.CubicOut),
            LogoBranding.ScaleTo(1, 650, Easing.SpringOut),
            LogoBranding.TranslateTo(0, 0, 520, Easing.CubicOut)
        );

        // ── Fase 2: Botón de acción sube desde abajo ──
        await Task.WhenAll(
            ActionSection.FadeTo(1, 380, Easing.CubicOut),
            ActionSection.TranslateTo(0, 0, 380, Easing.CubicOut)
        );
    }
}
