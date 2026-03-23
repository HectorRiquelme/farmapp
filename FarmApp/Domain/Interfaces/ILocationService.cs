using FarmApp.Domain.Models;

namespace FarmApp.Domain.Interfaces;

/// <summary>
/// Abstracción de geolocalización del dispositivo.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Obtiene la ubicación actual del usuario.
    /// Retorna null si no hay permiso o la ubicación no está disponible.
    /// </summary>
    Task<UbicacionUsuario?> ObtenerUbicacionActualAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si la app tiene permiso de ubicación concedido.
    /// </summary>
    Task<bool> TienePermisoUbicacionAsync();
}
