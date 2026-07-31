using FarmApp.Domain.Models;

namespace FarmApp.Domain.Interfaces;

/// <summary>
/// Estado y operaciones de FarmApp Pro: compra única que quita los anuncios
/// y desbloquea preferencias avanzadas (radio recordado, tema manual).
/// </summary>
public interface IProService
{
    /// <summary>True si el usuario compró FarmApp Pro (caché local en Preferences).</summary>
    bool EsPro { get; }

    /// <summary>Se dispara cuando cambia el estado Pro (compra, restauración o revocación).</summary>
    event EventHandler<bool>? EsProCambiado;

    /// <summary>Lanza el flujo de compra nativo de Google Play.</summary>
    Task<ResultadoCompraPro> ComprarProAsync();

    /// <summary>Consulta compras previas de la cuenta Google y reactiva Pro si corresponde.</summary>
    Task<ResultadoCompraPro> RestaurarComprasAsync();

    /// <summary>Precio localizado del producto Pro (ej: "CLP $2.990"), o null si no se pudo obtener.</summary>
    Task<string?> ObtenerPrecioLocalizadoAsync();

    /// <summary>
    /// Revalida en segundo plano una compra ya registrada (detecta reembolsos).
    /// Nunca lanza excepciones; pensada para el arranque de la app.
    /// </summary>
    Task RevalidarSilenciosoAsync();
}
