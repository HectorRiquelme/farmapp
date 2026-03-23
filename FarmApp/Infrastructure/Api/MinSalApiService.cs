using System.Text.Json;
using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;
using FarmApp.Infrastructure.Api.Dtos;
using Microsoft.Extensions.Logging;

namespace FarmApp.Infrastructure.Api;

/// <summary>
/// Implementación concreta del proveedor MIDAS/MINSAL.
///
/// Comportamiento confirmado de la API real:
/// - Responde un array JSON directo (sin wrapper)
/// - Incluye turno del día actual y del día siguiente
/// - Urgencias identificadas por localidad_nombre con sufijo "URGENCIA"
/// - Horarios en formato "HH:mm:ss"
/// - Todos los registros tienen coordenadas GPS (en datos observados)
/// </summary>
public class MinSalApiService : IFarmaciaProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MinSalApiService> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MinSalApiService(HttpClient httpClient, ILogger<MinSalApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Farmacia>> ObtenerFarmaciasAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consultando API MIDAS: {Url}", AppConstants.MinSalApiUrl);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(AppConstants.ApiTimeoutSegundos));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(AppConstants.MinSalApiUrl, cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Timeout al consultar API MIDAS (>{Segundos}s)", AppConstants.ApiTimeoutSegundos);
            throw new TimeoutException($"La API no respondió en {AppConstants.ApiTimeoutSegundos} segundos.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al consultar API MIDAS");
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API MIDAS retornó HTTP {StatusCode}", response.StatusCode);
            throw new HttpRequestException($"API retornó error: {(int)response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParsearRespuesta(json);
    }

    private List<Farmacia> ParsearRespuesta(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogWarning("API MIDAS retornó respuesta vacía");
            return [];
        }

        try
        {
            // La API real retorna un array JSON directo
            var arreglo = JsonSerializer.Deserialize<List<MidasFarmaciaDto>>(json, JsonOpts);

            if (arreglo is { Count: > 0 })
            {
                _logger.LogInformation("MIDAS retornó {Total} farmacias", arreglo.Count);
                return arreglo
                    .Select(ApiNormalizer.FromMidasDto)
                    .ToList();
            }

            _logger.LogWarning("API MIDAS retornó array vacío");
            return [];
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error al deserializar respuesta de MIDAS. Primeros 200 chars: {Json}",
                json[..Math.Min(200, json.Length)]);
            throw new InvalidOperationException("La respuesta de la API no pudo interpretarse.", ex);
        }
    }
}
