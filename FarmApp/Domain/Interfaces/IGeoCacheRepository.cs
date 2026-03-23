namespace FarmApp.Domain.Interfaces;

public interface IGeoCacheRepository
{
    Task<(double Lat, double Lon)?> ObtenerCoordenadaAsync(string claveDireccion);

    Task GuardarCoordenadaAsync(string claveDireccion, double lat, double lon);
}
