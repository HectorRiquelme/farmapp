using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;
using Microsoft.Extensions.Logging;
using SQLite;

namespace FarmApp.Infrastructure.Cache;

/// <summary>
/// Repositorio SQLite local. Persiste las farmacias para uso offline.
/// Usa la conexión compartida DatabaseConnection para evitar contención
/// con GeoCacheRepository durante escrituras concurrentes.
/// </summary>
public class FarmaciaRepository : IFarmaciaRepository
{
    private readonly SQLiteAsyncConnection _db;
    private readonly ILogger<FarmaciaRepository> _logger;

    public FarmaciaRepository(DatabaseConnection conexion, ILogger<FarmaciaRepository> logger)
    {
        _logger = logger;
        _db = conexion.Db;
        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        await _db.CreateTableAsync<Farmacia>();
        await _db.CreateTableAsync<GeoCache>();
    }

    public async Task GuardarLoteAsync(List<Farmacia> farmacias)
    {
        if (farmacias.Count == 0)
            return;

        try
        {
            await _db.RunInTransactionAsync(db =>
            {
                // Reemplaza todos los registros del día actual
                db.DeleteAll<Farmacia>();
                foreach (var f in farmacias)
                    db.InsertOrReplace(f);
            });

            _logger.LogInformation("Guardadas {Count} farmacias en caché local", farmacias.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar farmacias en SQLite");
        }
    }

    public async Task<List<Farmacia>> ObtenerUltimasAsync()
    {
        try
        {
            return await _db.Table<Farmacia>().ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer farmacias desde SQLite");
            return [];
        }
    }

    public async Task<DateTime?> ObtenerFechaUltimaConsultaAsync()
    {
        try
        {
            var ultima = await _db.Table<Farmacia>()
                .OrderByDescending(f => f.FechaConsulta)
                .FirstOrDefaultAsync();

            return ultima?.FechaConsulta;
        }
        catch
        {
            return null;
        }
    }

    public async Task LimpiarRegistrosViejosAsync(int diasMaximos = 2)
    {
        try
        {
            var limite = DateTime.Now.AddDays(-diasMaximos);
            await _db.Table<Farmacia>()
                .DeleteAsync(f => f.FechaConsulta < limite);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al limpiar registros viejos de SQLite");
        }
    }
}
