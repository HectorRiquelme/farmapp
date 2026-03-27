using FarmApp.Presentation.Controls;
using FarmApp.Presentation.ViewModels;

namespace FarmApp.Presentation.Pages;

public partial class ResultadosPage : ContentPage
{
    public ResultadosPage(ResultadosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = (ResultadosViewModel)BindingContext;
        vm.PropertyChanged += OnViewModelPropertyChanged;

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
}
