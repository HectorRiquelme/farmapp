using System.Text.Json.Serialization;

namespace FarmApp.Infrastructure.Api.Dtos;

/// <summary>
/// DTO que mapea la respuesta real de la API MIDAS/MINSAL.
/// Validado contra el endpoint: GET https://midas.minsal.cl/farmacia_v2/WS/getLocalesTurnos.php
/// La respuesta es un array JSON directo (sin wrapper).
///
/// Patrones de horario observados en datos reales:
///   "00:00:00" – "23:59:00"  → urgencia / permanente (casi 24h)
///   "09:00:00" – "08:59:00"  → turno full-day (cruza medianoche, 1 min de "cierre" técnico)
///   "21:00:00" – "09:00:00"  → turno nocturno clásico
///
/// Urgencias: localidad_nombre contiene el sufijo "URGENCIA"
/// </summary>
public class MidasFarmaciaDto
{
    [JsonPropertyName("local_id")]
    public string? LocalId { get; set; }

    [JsonPropertyName("local_nombre")]
    public string? LocalNombre { get; set; }

    [JsonPropertyName("local_direccion")]
    public string? LocalDireccion { get; set; }

    [JsonPropertyName("comuna_nombre")]
    public string? ComunaNombre { get; set; }

    /// <summary>
    /// Contiene el nombre de la localidad. Si termina en "URGENCIA",
    /// la farmacia es de urgencia permanente.
    /// </summary>
    [JsonPropertyName("localidad_nombre")]
    public string? LocalidadNombre { get; set; }

    [JsonPropertyName("local_telefono")]
    public string? LocalTelefono { get; set; }

    [JsonPropertyName("local_lat")]
    public string? LocalLat { get; set; }

    [JsonPropertyName("local_lng")]
    public string? LocalLng { get; set; }

    /// <summary>Formato real: "HH:mm:ss"</summary>
    [JsonPropertyName("funcionamiento_hora_apertura")]
    public string? FuncionamientoHoraApertura { get; set; }

    /// <summary>Formato real: "HH:mm:ss"</summary>
    [JsonPropertyName("funcionamiento_hora_cierre")]
    public string? FuncionamientoHoraCierre { get; set; }

    /// <summary>Fecha del turno. Urgencias usan "2026-01-01" como fecha fija.</summary>
    [JsonPropertyName("fecha")]
    public string? Fecha { get; set; }

    /// <summary>Día de la semana del turno (ej: "lunes").</summary>
    [JsonPropertyName("funcionamiento_dia")]
    public string? FuncionamientoDia { get; set; }

    // Campos de FK (códigos numéricos, no usados en MVP)
    [JsonPropertyName("fk_region")]
    public string? FkRegion { get; set; }

    [JsonPropertyName("fk_comuna")]
    public string? FkComuna { get; set; }
}
