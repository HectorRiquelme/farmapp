using FarmApp.Domain.Models;

namespace FarmApp.Domain.Interfaces;

/// <summary>
/// Contrato para cualquier proveedor de datos de farmacias (MIDAS, SEREMI, etc.).
/// Permite cambiar la fuente de datos sin afectar capas superiores.
/// </summary>
public interface IFarmaciaProvider
{
    /// <summary>
    /// Obtiene la lista de farmacias de turno/urgencia disponibles.
    /// Lanza excepción si la fuente no está disponible.
    /// </summary>
    Task<List<Farmacia>> ObtenerFarmaciasAsync(CancellationToken cancellationToken = default);
}
