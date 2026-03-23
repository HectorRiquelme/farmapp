using FarmApp.Constants;
using SQLite;

namespace FarmApp.Infrastructure.Cache;

/// <summary>
/// Conexión SQLite compartida entre todos los repositorios.
/// Registrada como Singleton en DI — una sola instancia para toda la vida de la app.
/// SQLiteAsyncConnection serializa internamente todas las operaciones async,
/// eliminando el riesgo de SQLiteBusyException por escrituras concurrentes.
/// </summary>
public class DatabaseConnection
{
    public SQLiteAsyncConnection Db { get; }

    public DatabaseConnection()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, AppConstants.NombreBaseDatos);
        Db = new SQLiteAsyncConnection(dbPath);
    }
}
