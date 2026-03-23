using System.Text.Json;
using System.Text.Json.Serialization;
using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FarmApp.Infrastructure.Api;

/// <summary>
/// Geocodifica direcciones usando Nominatim (OpenStreetMap).
/// Uso permitido con User-Agent identificado. Rate limit: 1 req/s.
/// Para producción con volumen alto, considerar HERE Maps o Google Geocoding API.
/// </summary>
public class GeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly IGeoCacheRepository _geoCache;
    private readonly ILogger<GeocodingService> _logger;

    // Throttle simple: mínimo 1 segundo entre requests a Nominatim
    private DateTime _ultimaConsulta = DateTime.MinValue;
    private readonly SemaphoreSlim _throttle = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public GeocodingService(
        HttpClient httpClient,
        IGeoCacheRepository geoCache,
        ILogger<GeocodingService> logger)
    {
        _httpClient = httpClient;
        _geoCache = geoCache;
        _logger = logger;
    }

    public async Task<(double Lat, double Lon)?> GeocodificarAsync(
        string direccion,
        string comuna,
        CancellationToken cancellationToken = default)
    {
        var clave = NormalizarClave(direccion, comuna);

        // Verificar caché primero
        var cached = await _geoCache.ObtenerCoordenadaAsync(clave);
        if (cached.HasValue)
            return cached;

        var query = Uri.EscapeDataString($"{direccion}, {comuna}, Chile");
        var url = $"{AppConstants.NominatimBaseUrl}?q={query}&format=json&limit=1&countrycodes=cl";

        await _throttle.WaitAsync(cancellationToken);
        try
        {
            // Respetar rate limit de Nominatim
            var espera = TimeSpan.FromSeconds(1) - (DateTime.Now - _ultimaConsulta);
            if (espera > TimeSpan.Zero)
                await Task.Delay(espera, cancellationToken);

            _ultimaConsulta = DateTime.Now;

            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim retornó {StatusCode} para {Direccion}", response.StatusCode, direccion);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var resultados = JsonSerializer.Deserialize<List<NominatimResultado>>(json, JsonOpts);

            if (resultados is not { Count: > 0 })
                return null;

            var r = resultados[0];
            if (!double.TryParse(r.Lat, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lat))
                return null;

            if (!double.TryParse(r.Lon, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var lon))
                return null;

            await _geoCache.GuardarCoordenadaAsync(clave, lat, lon);
            return (lat, lon);
        }
        catch (OperationCanceledException)
        {
            // Propagamos para que el caller (BuscarFarmaciasUseCase) maneje el break del loop
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al geocodificar: {Direccion}, {Comuna}", direccion, comuna);
            return null;
        }
        finally
        {
            _throttle.Release();
        }
    }

    private static string NormalizarClave(string direccion, string comuna) =>
        $"{direccion.Trim().ToLowerInvariant()}|{comuna.Trim().ToLowerInvariant()}";

    private class NominatimResultado
    {
        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}
