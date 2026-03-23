using FarmApp.Domain.Models;
using FarmApp.Infrastructure.Api.Dtos;

namespace FarmApp.Infrastructure.Api;

/// <summary>
/// Transforma DTOs reales de la API MIDAS al modelo de dominio interno.
///
/// Reglas de negocio aplicadas aquí:
/// - Urgencia: localidad_nombre contiene "URGENCIA"
/// - Horario "HH:mm:ss" parseado a TimeSpan
/// - Teléfono normalizado a formato legible
/// - Texto en MAYÚSCULAS convertido a Title Case
/// - Coordenadas string → double nullable
/// </summary>
public static class ApiNormalizer
{
    public static Farmacia FromMidasDto(MidasFarmaciaDto dto)
    {
        var id = string.IsNullOrWhiteSpace(dto.LocalId)
            ? Guid.NewGuid().ToString()
            : $"midas_{dto.LocalId}";

        var apertura = ParseHora(dto.FuncionamientoHoraApertura);
        var cierre = ParseHora(dto.FuncionamientoHoraCierre);
        var esUrgencia = EsUrgencia(dto);

        var horarioTexto = apertura.HasValue && cierre.HasValue
            ? FormatearHorario(apertura.Value, cierre.Value, esUrgencia)
            : "Horario no informado";

        return new Farmacia
        {
            Id = id,
            Nombre = Capitalizar(dto.LocalNombre?.Trim() ?? "Sin nombre"),
            Direccion = Capitalizar(dto.LocalDireccion?.Trim() ?? ""),
            Comuna = Capitalizar(dto.ComunaNombre?.Trim() ?? "Desconocida"),
            Region = string.Empty, // La API solo entrega fk_region (código numérico)
            Latitud = ParseCoordenada(dto.LocalLat),
            Longitud = ParseCoordenada(dto.LocalLng),
            Telefono = NormalizarTelefono(dto.LocalTelefono),
            HorarioTexto = horarioTexto,
            Apertura = apertura,
            Cierre = cierre,
            Tipo = esUrgencia ? TipoFarmacia.Urgencia : TipoFarmacia.Turno,
            Fuente = "MIDAS",
            FechaConsulta = DateTime.Now,
            Estado = EstadoApertura.SinDatos, // se calcula en AperturaService
            Observaciones = dto.FuncionamientoDia ?? string.Empty
        };
    }

    /// <summary>
    /// Urgencia si localidad_nombre contiene "URGENCIA" (patrón confirmado en datos reales).
    /// Ejemplos: "VIÑA DEL MAR URGENCIA", "VALPARAISO URGENCIA", "RANCAGUA URGENCIA"
    /// </summary>
    private static bool EsUrgencia(MidasFarmaciaDto dto)
    {
        var localidad = dto.LocalidadNombre?.ToUpperInvariant() ?? "";
        return localidad.Contains("URGENCIA");
    }

    /// <summary>
    /// Parsea horario en formato "HH:mm:ss" (formato real de la API).
    /// </summary>
    private static TimeSpan? ParseHora(string? horarioTexto)
    {
        if (string.IsNullOrWhiteSpace(horarioTexto))
            return null;

        // Formato real: "HH:mm:ss"
        if (TimeSpan.TryParseExact(horarioTexto.Trim(),
            ["HH\\:mm\\:ss", "H\\:mm\\:ss", "HH\\:mm", "H\\:mm"],
            null, out var resultado))
            return resultado;

        // Fallback genérico
        if (TimeSpan.TryParse(horarioTexto.Trim(), out var resultado2))
            return resultado2;

        return null;
    }

    /// <summary>
    /// Convierte el texto de horario a un formato legible para el usuario.
    /// Detecta el patrón "turno full-day" donde cierre = apertura - 1 minuto
    /// (ej: 09:00–08:59 o 08:00–07:59), que indica farmacia de turno todo el día.
    /// </summary>
    private static string FormatearHorario(TimeSpan apertura, TimeSpan cierre, bool esUrgencia)
    {
        if (esUrgencia || (apertura == TimeSpan.Zero && cierre == new TimeSpan(23, 59, 0)))
            return "Abierta 24 horas";

        // Patrón full-day: cierre es 1 minuto antes que apertura (cruza medianoche)
        var diferencia = (cierre - apertura).TotalMinutes;
        if (Math.Abs(diferencia + 1) < 1 || Math.Abs(diferencia - (24 * 60 - 1)) < 1)
            return $"Turno todo el día ({apertura:hh\\:mm} – {apertura:hh\\:mm} siguiente día)";

        return $"{apertura:hh\\:mm} – {cierre:hh\\:mm}";
    }

    private static double? ParseCoordenada(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        if (!double.TryParse(valor.Trim(),
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var resultado))
            return null;

        return resultado == 0.0 ? null : resultado;
    }

    /// <summary>
    /// Normaliza teléfonos al formato E.164 dialable.
    /// La API MIDAS entrega formatos muy variados y algunos inválidos.
    /// Si el número no puede normalizarse a algo útil, retorna string.Empty
    /// para que la UI oculte el botón de llamada.
    ///
    /// Patrones observados en datos reales:
    ///   "+56322588998" (11 dígitos con 56) → válido
    ///   "+560226313145" (12 dígitos con 560, trunk legacy) → quitar el 0
    ///   "+5633232740626" (13+ dígitos) → probablemente inválido
    ///   "+56", "+560", "+560987" (menos de 8 dígitos útiles) → inválido
    /// </summary>
    private static string NormalizarTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return string.Empty;

        var soloDigitos = new string(telefono.Where(char.IsDigit).ToArray());

        // Menos de 7 dígitos → inútil para marcar
        if (soloDigitos.Length < 7)
            return string.Empty;

        // Quitar prefijo de país "56" para trabajar con el número local
        string numero;
        if (soloDigitos.StartsWith("560") && soloDigitos.Length == 12)
        {
            // "+560226313145" → quitar el 0 de trunk → "56226313145"
            numero = "56" + soloDigitos[3..];
        }
        else if (soloDigitos.StartsWith("56") && soloDigitos.Length >= 10)
        {
            numero = soloDigitos;
        }
        else
        {
            // Número local sin código país, agregar +56
            numero = "56" + soloDigitos;
        }

        // Validar largo razonable para Chile (10–11 dígitos con código país)
        if (numero.Length < 10 || numero.Length > 12)
            return string.Empty;

        return $"+{numero}";
    }

    private static string Capitalizar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return texto;

        // Si tiene minúsculas, dejarlo como viene (ya capitalizado)
        if (texto.Any(char.IsLower))
            return texto;

        // Todo en mayúsculas → convertir a Title Case
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(texto.ToLower());
    }
}
