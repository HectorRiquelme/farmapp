using FarmApp.Domain.Models;

namespace FarmApp.Domain.Interfaces;

/// <summary>
/// Repositorio local de farmacias cacheadas en SQLite.
/// </summary>
public interface IFarmaciaRepository
{
    Task GuardarLoteAsync(List<Farmacia> farmacias);

    Task<List<Farmacia>> ObtenerUltimasAsync();

    Task<DateTime?> ObtenerFechaUltimaConsultaAsync();

    Task LimpiarRegistrosViejosAsync(int diasMaximos = 2);
}
