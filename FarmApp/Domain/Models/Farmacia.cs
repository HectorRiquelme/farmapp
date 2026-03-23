using SQLite;

namespace FarmApp.Domain.Models;

[Table("Farmacias")]
public class Farmacia
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string Comuna { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public double? Latitud { get; set; }

    public double? Longitud { get; set; }

    public string Telefono { get; set; } = string.Empty;

    // Texto crudo de horario tal como viene de la API
    public string HorarioTexto { get; set; } = string.Empty;

    // Almacenado como minutos desde medianoche para SQLite (null = no disponible)
    public int? AperturaMinutos { get; set; }

    public int? CierreMinutos { get; set; }

    public TipoFarmacia Tipo { get; set; } = TipoFarmacia.NoDefinido;

    public string Fuente { get; set; } = string.Empty;

    public DateTime FechaConsulta { get; set; }

    public EstadoApertura Estado { get; set; } = EstadoApertura.SinDatos;

    public string Observaciones { get; set; } = string.Empty;

    // --- Propiedades calculadas en runtime (no persistidas en SQLite) ---

    [Ignore]
    public TimeSpan? Apertura
    {
        get => AperturaMinutos.HasValue ? TimeSpan.FromMinutes(AperturaMinutos.Value) : null;
        set => AperturaMinutos = value.HasValue ? (int)value.Value.TotalMinutes : null;
    }

    [Ignore]
    public TimeSpan? Cierre
    {
        get => CierreMinutos.HasValue ? TimeSpan.FromMinutes(CierreMinutos.Value) : null;
        set => CierreMinutos = value.HasValue ? (int)value.Value.TotalMinutes : null;
    }

    [Ignore]
    public double? DistanciaKm { get; set; }

    [Ignore]
    public bool TieneCoordenadas =>
        Latitud.HasValue && Longitud.HasValue &&
        Latitud.Value != 0 && Longitud.Value != 0;

    [Ignore]
    public bool TieneTelefono =>
        !string.IsNullOrWhiteSpace(Telefono);

    [Ignore]
    public string DistanciaTexto => DistanciaKm.HasValue
        ? DistanciaKm.Value < 1
            ? $"{(int)(DistanciaKm.Value * 1000)} m"
            : $"{DistanciaKm.Value:F1} km"
        : "Distancia no disponible";

    [Ignore]
    public string EstadoTexto => Estado switch
    {
        EstadoApertura.AbiertaAhora => "Abierta ahora",
        EstadoApertura.PosiblementeAbierta => "Posiblemente abierta",
        EstadoApertura.HorarioNoConfirmado => "Horario no confirmado",
        EstadoApertura.Cerrada => "Cerrada",
        EstadoApertura.SinDatos => "Sin datos",
        _ => "Sin datos"
    };

    [Ignore]
    public string TipoTexto => Tipo switch
    {
        TipoFarmacia.Turno => "Farmacia de turno",
        TipoFarmacia.Urgencia => "Farmacia de urgencia",
        _ => "Farmacia"
    };

    /// <summary>
    /// Indica en runtime si esta farmacia es la más cercana de la lista mostrada.
    /// Lo asigna el ViewModel; no se persiste en SQLite.
    /// </summary>
    [Ignore]
    public bool EsMasCercana { get; set; }
}
