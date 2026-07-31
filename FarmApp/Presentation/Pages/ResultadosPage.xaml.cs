using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using FarmApp.Presentation.Controls;
using FarmApp.Presentation.ViewModels;
using Plugin.MauiMtAdmob.Controls;

namespace FarmApp.Presentation.Pages;

public partial class ResultadosPage : ContentPage
{
    private readonly IProService _proService;

    public ResultadosPage(ResultadosViewModel viewModel, IProService proService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _proService = proService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = (ResultadosViewModel)BindingContext;
        vm.PropertyChanged += OnViewModelPropertyChanged;

        ActualizarZonaBanner();

        if (vm.TieneResultados)
        {
            MiniMapa.LoadFarmacias(vm.TodasLasFarmacias);
            EnviarUbicacionUsuarioAlMapa(vm);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        var vm = (ResultadosViewModel)BindingContext;
        vm.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        var vm = (ResultadosViewModel)BindingContext;

        switch (e.PropertyName)
        {
            // Lista actualizada (carga inicial o slider) → recargar todos los pins
            case nameof(ResultadosViewModel.MapaVersion):
                if (vm.TieneResultados)
                {
                    MiniMapa.LoadFarmacias(vm.TodasLasFarmacias);
                    EnviarUbicacionUsuarioAlMapa(vm);
                }
                break;

            // Lista actualizada por slider → también enviar ubicación del usuario
            // (el mapa se recarga desde cero con loadFarmacias)

            // Card tocada → centrar mapa + resaltar card correspondiente
            case nameof(ResultadosViewModel.FarmaciaSeleccionadaEnMapaId):
                var selectedId = vm.FarmaciaSeleccionadaEnMapaId;
                if (!string.IsNullOrEmpty(selectedId))
                    MiniMapa.CentrarEn(selectedId);
                ActualizarSeleccionEnLista(selectedId);
                break;
        }
    }

    /// <summary>
    /// Envía la ubicación del usuario al mapa para mostrar el pin azul "Tú estás aquí".
    /// </summary>
    private void EnviarUbicacionUsuarioAlMapa(ResultadosViewModel vm)
    {
        var ubicacion = vm.ResultadoBusqueda?.UbicacionUsuario;
        if (ubicacion != null)
            MiniMapa.SetUserLocation(ubicacion.Latitud, ubicacion.Longitud);
    }

    /// <summary>
    /// Recorre las FarmaciaCardCompacta del BindableLayout y actualiza IsSelected.
    /// </summary>
    private void ActualizarSeleccionEnLista(string selectedId)
    {
        foreach (var item in ListaFarmacias.Children)
        {
            if (item is FarmaciaCardCompacta card)
                card.IsSelected = card.Farmacia?.Id == selectedId;
        }
    }

    // ─────────────────────────────────────────────────────
    //  Banner AdMob (solo versión gratuita)
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Construye el banner únicamente para usuarios sin Pro. Si el usuario
    /// compra Pro (incluso a mitad de sesión), la zona queda vacía al volver.
    /// </summary>
    private void ActualizarZonaBanner()
    {
        if (_proService.EsPro)
        {
            ZonaBanner.Children.Clear();
            return;
        }

        if (ZonaBanner.Children.Count > 0) return;

        // Enlace discreto para quitar los anuncios comprando Pro
        var enlacePro = new Label
        {
            Text = "Quitar anuncios · FarmApp Pro ⭐",
            FontSize = 11,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(8, 6, 8, 4)
        };
        enlacePro.SetAppThemeColor(Label.TextColorProperty,
            ObtenerColorRecurso("ColorTagCategoriaLight"),
            ObtenerColorRecurso("ColorTagCategoria"));

        var irAPro = new TapGestureRecognizer();
        irAPro.Tapped += async (_, _) => await Shell.Current.GoToAsync(nameof(ProPage));
        enlacePro.GestureRecognizers.Add(irAPro);

        var banner = new MTAdView
        {
            AdsId = MonetizacionConstants.AdMobBannerResultadosId
        };

        ZonaBanner.Children.Add(enlacePro);
        ZonaBanner.Children.Add(banner);
    }

    private static Color ObtenerColorRecurso(string clave) =>
        Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(clave, out var valor) == true
            && valor is Color color ? color : Colors.Gray;
}
