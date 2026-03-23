using FarmApp.Domain.Models;

namespace FarmApp.Domain.Services;

/// <summary>
/// Determina el estado de apertura de una farmacia basado en el horario actual.
/// Lógica conservadora: si hay duda, NO se asume abierta.
///
/// Patrones de horario reales observados en API MIDAS:
///   00:00–23:59 → urgencia / permanente (abierta todo el día)
///   09:00–08:59 → turno todo el día (cruza medianoche, 1 min de gap técnico)
///   08:00–07:59 → turno todo el día (ídem)
///   21:00–09:00 → turno nocturno clásico
/// </summary>
public class AperturaService
{
    // Los datos de la API son válidos si fueron consultados hace menos de 26 horas
    private static readonly TimeSpan ToleranciaDatos = TimeSpan.FromHours(26);

    public EstadoApertura Determinar(Farmacia farmacia, DateTime ahora)
    {
        // Datos muy viejos: no podemos confirmar
        if (ahora - farmacia.FechaConsulta > ToleranciaDatos)
            return EstadoApertura.HorarioNoConfirmado;

        if (!farmacia.Apertura.HasValue || !farmacia.Cierre.HasValue)
            return EstadoApertura.HorarioNoConfirmado;

        var apertura = farmacia.Apertura.Value;
        var cierre = farmacia.Cierre.Value;

        // Caso especial: "turno todo el día" (cierre = apertura - 1 min, cruza medianoche)
        // Ej: 09:00–08:59, 08:00–07:59. En la práctica está siempre abierta.
        if (EsTurnoTodoDia(apertura, cierre))
        {
            return farmacia.FechaConsulta.Date == ahora.Date
                ? EstadoApertura.AbiertaAhora
                : EstadoApertura.PosiblementeAbierta;
        }

        bool estaAbierta = EstaEnHorario(ahora.TimeOfDay, apertura, cierre);

        if (!estaAbierta)
            return EstadoApertura.Cerrada;

        // Abierta confirmada solo si los datos son del día actual o siguiente
        bool datosRecientes = (ahora - farmacia.FechaConsulta).TotalHours <= 26;
        bool datosDeHoyOManana = farmacia.FechaConsulta.Date >= ahora.Date.AddDays(-1);

        return datosRecientes && datosDeHoyOManana
            ? EstadoApertura.AbiertaAhora
            : EstadoApertura.PosiblementeAbierta;
    }

    /// <summary>
    /// Detecta el patrón "turno todo el día": cierre es exactamente 1 minuto antes que apertura.
    /// La API MIDAS usa esta convención: 09:00–08:59 significa abierta 24h técnicamente.
    /// </summary>
    private static bool EsTurnoTodoDia(TimeSpan apertura, TimeSpan cierre)
    {
        var diferencia = apertura - cierre;
        return diferencia == TimeSpan.FromMinutes(1);
    }

    /// <summary>
    /// Determina si la hora actual está dentro del horario de atención.
    /// Maneja correctamente los horarios que cruzan medianoche (ej: 21:00–09:00).
    /// </summary>
    private static bool EstaEnHorario(TimeSpan horaActual, TimeSpan apertura, TimeSpan cierre)
    {
        // Horario normal: apertura antes que cierre (ej: 09:00–18:00)
        if (apertura <= cierre)
            return horaActual >= apertura && horaActual < cierre;

        // Horario cruzando medianoche (ej: 21:00–09:00):
        // está abierta si hora >= 21:00 OR hora < 09:00
        return horaActual >= apertura || horaActual < cierre;
    }

    public bool EsVisibleEnLista(EstadoApertura estado) =>
        estado != EstadoApertura.Cerrada;
}
