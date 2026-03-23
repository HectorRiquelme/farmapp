using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FarmApp.Infrastructure.Location;

/// <summary>
/// Implementación de geolocalización usando MAUI Geolocation API.
/// </summary>
public class MauiLocationService : ILocationService
{
    private readonly ILogger<MauiLocationService> _logger;

    public MauiLocationService(ILogger<MauiLocationService> logger)
    {
        _logger = logger;
    }

    public async Task<UbicacionUsuario?> ObtenerUbicacionActualAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var permiso = await TienePermisoUbicacionAsync();
            if (!permiso)
            {
                _logger.LogWarning("Permiso de ubicación no concedido");
                return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(15));
            var location = await Geolocation.Default.GetLocationAsync(request, cancellationToken);

            if (location == null)
            {
                _logger.LogWarning("GetLocationAsync retornó null");
                return null;
            }

            _logger.LogInformation("Ubicación obtenida: {Lat}, {Lon}", location.Latitude, location.Longitude);
            return new UbicacionUsuario(location.Latitude, location.Longitude);
        }
        catch (FeatureNotSupportedException)
        {
            _logger.LogError("Geolocalización no disponible en este dispositivo");
            return null;
        }
        catch (PermissionException)
        {
            _logger.LogWarning("Permiso de ubicación denegado");
            return null;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Obtención de ubicación cancelada por timeout");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener ubicación");
            return null;
        }
    }

    public async Task<bool> TienePermisoUbicacionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied)
            return false;

        // Solicitar permiso si no ha sido determinado
        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }
}
