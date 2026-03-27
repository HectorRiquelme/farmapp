using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmApp.Application;
using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FarmApp.Presentation.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly BuscarFarmaciasUseCase _buscarFarmacias;
    private readonly ILocationService _locationService;
    private readonly ILogger<HomeViewModel> _logger;

    [ObservableProperty]
    private string _mensajeError = string.Empty;

    [ObservableProperty]
    private bool _tieneError;

    [ObservableProperty]
    private bool _tienePermiso = true;

    public HomeViewModel(
        BuscarFarmaciasUseCase buscarFarmacias,
        ILocationService locationService,
        ILogger<HomeViewModel> logger)
    {
        _buscarFarmacias = buscarFarmacias;
        _locationService = locationService;
        _logger = logger;
        Titulo = "FarmApp";
    }

    [RelayCommand]
    private async Task BuscarFarmaciasAsync()
    {
        if (EstaCargando) return;

        EstaCargando = true;
        TieneError = false;
        MensajeError = string.Empty;

        try
        {
            // Solicitar permiso de ubicación (muestra diálogo nativo automáticamente)
            var permiso = await _locationService.TienePermisoUbicacionAsync();
            TienePermiso = permiso;

            UbicacionUsuario? ubicacion = null;
            if (permiso)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                ubicacion = await _locationService.ObtenerUbicacionActualAsync(cts.Token);
            }

            // Buscar farmacias aunque no haya GPS (funciona sin ubicación, solo sin distancias)
            var resultado = await _buscarFarmacias.EjecutarAsync(ubicacion);

            if (resultado.TieneError)
            {
                MensajeError = resultado.Error!;
                TieneError = true;
                return;
            }

            // Navegar a resultados
            await Shell.Current.GoToAsync(nameof(Pages.ResultadosPage), new Dictionary<string, object>
            {
                ["ResultadoBusqueda"] = resultado
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al buscar farmacias");
            MensajeError = "Ocurrió un error inesperado. Intenta nuevamente.";
            TieneError = true;
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task SolicitarPermisoAsync()
    {
        var permiso = await _locationService.TienePermisoUbicacionAsync();
        TienePermiso = permiso;

        if (permiso)
        {
            // Permiso otorgado → lanzar búsqueda automáticamente
            await BuscarFarmaciasAsync();
        }
        else
        {
            // Permiso denegado permanentemente → abrir ajustes del sistema
            AppInfo.Current.ShowSettingsUI();
        }
    }
}
