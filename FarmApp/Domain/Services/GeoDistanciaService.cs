using FarmApp.Domain.Models;

namespace FarmApp.Domain.Services;

/// <summary>
/// Cálculo de distancia geográfica usando la fórmula de Haversine.
/// Precisión suficiente para radio urbano de menos de 100 km.
/// </summary>
public class GeoDistanciaService
{
    private const double RadioTierraKm = 6371.0;

    public double CalcularKm(UbicacionUsuario origen, double destLat, double destLon)
    {
        var dLat = ToRad(destLat - origen.Latitud);
        var dLon = ToRad(destLon - origen.Longitud);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(origen.Latitud)) * Math.Cos(ToRad(destLat))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return RadioTierraKm * c;
    }

    public void AsignarDistancias(List<Farmacia> farmacias, UbicacionUsuario ubicacion)
    {
        foreach (var f in farmacias)
        {
            if (f.TieneCoordenadas)
                f.DistanciaKm = CalcularKm(ubicacion, f.Latitud!.Value, f.Longitud!.Value);
        }
    }

    public List<Farmacia> FiltrarYOrdenarPorRadio(
        List<Farmacia> farmacias,
        UbicacionUsuario ubicacion,
        double radioKm)
    {
        // Solo incluir farmacias que tienen coordenadas Y están dentro del radio.
        // Ordenar priorizando abiertas/posiblemente abiertas primero, cerradas al final.
        // Dentro de cada grupo, ordenar por distancia ascendente.
        return farmacias
            .Where(f => f.DistanciaKm.HasValue && f.DistanciaKm.Value <= radioKm)
            .OrderBy(f => f.Estado == EstadoApertura.Cerrada ? 1 : 0)
            .ThenBy(f => f.DistanciaKm!.Value)
            .ToList();
    }

    private static double ToRad(double grados) => grados * Math.PI / 180.0;
}
