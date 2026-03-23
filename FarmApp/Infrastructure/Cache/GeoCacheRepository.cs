using FarmApp.Domain.Interfaces;
using SQLite;

namespace FarmApp.Infrastructure.Cache;

[Table("GeoCache")]
public class GeoCache
{
    [PrimaryKey]
    public string ClaveDireccion { get; set; } = string.Empty;

    public double Latitud { get; set; }

    public double Longitud { get; set; }

    public DateTime FechaGuardado { get; set; }
}

public class GeoCacheRepository : IGeoCacheRepository
{
    private readonly SQLiteAsyncConnection _db;

    /// <summary>
    /// Recibe la conexión compartida para operar sobre el mismo archivo
    /// que FarmaciaRepository sin riesgo de contención concurrente.
    /// La tabla GeoCache la crea FarmaciaRepository.InicializarAsync(),
    /// que siempre se ejecuta antes al arrancar la app.
    /// </summary>
    public GeoCacheRepository(DatabaseConnection conexion)
    {
        _db = conexion.Db;
    }

    public async Task<(double Lat, double Lon)?> ObtenerCoordenadaAsync(string claveDireccion)
    {
        var entry = await _db.Table<GeoCache>()
            .Where(g => g.ClaveDireccion == claveDireccion)
            .FirstOrDefaultAsync();

        if (entry == null)
            return null;

        return (entry.Latitud, entry.Longitud);
    }

    public async Task GuardarCoordenadaAsync(string claveDireccion, double lat, double lon)
    {
        await _db.InsertOrReplaceAsync(new GeoCache
        {
            ClaveDireccion = claveDireccion,
            Latitud = lat,
            Longitud = lon,
            FechaGuardado = DateTime.Now
        });
    }
}
