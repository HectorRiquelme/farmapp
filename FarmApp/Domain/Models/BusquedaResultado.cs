namespace FarmApp.Domain.Models;

public class BusquedaResultado
{
    /// <summary>Farmacias filtradas por radio progresivo (resultado final del UseCase).</summary>
    public List<Farmacia> Farmacias { get; init; } = [];

    /// <summary>
    /// Todas las farmacias que tienen coordenadas, ordenadas por estado+distancia,
    /// sin límite de radio. Usado por el slider de búsqueda en la UI.
    /// </summary>
    public List<Farmacia> TodasConDistancia { get; init; } = [];

    public FuenteBusqueda Fuente { get; init; }

    public string? Advertencia { get; init; }

    public string? Error { get; init; }

    public bool TieneError => !string.IsNullOrEmpty(Error);

    public bool TieneAdvertencia => !string.IsNullOrEmpty(Advertencia);

    public bool TieneResultados => Farmacias.Count > 0;

    public Farmacia? MasCercana => Farmacias.FirstOrDefault();

    public static BusquedaResultado ConError(string error) =>
        new() { Error = error, Fuente = FuenteBusqueda.SinResultados };

    public static BusquedaResultado SinResultados(FuenteBusqueda fuente, string? advertencia = null) =>
        new() { Fuente = fuente, Advertencia = advertencia };

    public static BusquedaResultado Exitoso(
        List<Farmacia> farmacias,
        FuenteBusqueda fuente,
        string? advertencia = null,
        List<Farmacia>? todasConDistancia = null) =>
        new()
        {
            Farmacias = farmacias,
            Fuente = fuente,
            Advertencia = advertencia,
            TodasConDistancia = todasConDistancia ?? farmacias
        };
}
