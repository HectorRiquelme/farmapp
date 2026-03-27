using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;
using FarmApp.Domain.Services;
using FarmApp.Infrastructure.Api;
using Microsoft.Extensions.Logging;

namespace FarmApp.Application;

/// <summary>
/// Orquesta el flujo completo de búsqueda:
/// 1. Verifica conectividad
/// 2. Consulta API o caché
/// 3. Normaliza, calcula estado y distancias
/// 4. Filtra por radio progresivo
/// 5. Persiste en caché local
/// </summary>
public class BuscarFarmaciasUseCase
{
    private readonly IFarmaciaProvider _farmaciaProvider;
    private readonly IFarmaciaRepository _farmaciaRepository;
    private readonly AperturaService _aperturaService;
    private readonly GeoDistanciaService _geoDistanciaService;
    private readonly GeocodingService _geocodingService;
    private readonly ILogger<BuscarFarmaciasUseCase> _logger;

    public BuscarFarmaciasUseCase(
        IFarmaciaProvider farmaciaProvider,
        IFarmaciaRepository farmaciaRepository,
        AperturaService aperturaService,
        GeoDistanciaService geoDistanciaService,
        GeocodingService geocodingService,
        ILogger<BuscarFarmaciasUseCase> logger)
    {
        _farmaciaProvider = farmaciaProvider;
        _farmaciaRepository = farmaciaRepository;
        _aperturaService = aperturaService;
        _geoDistanciaService = geoDistanciaService;
        _geocodingService = geocodingService;
        _logger = logger;
    }

    public async Task<BusquedaResultado> EjecutarAsync(
        UbicacionUsuario? ubicacion,
        CancellationToken cancellationToken = default)
    {
        // --- Limpieza preventiva de registros viejos (no bloqueante) ---
        _ = _farmaciaRepository.LimpiarRegistrosViejosAsync();

        var hayConexion = Connectivity.NetworkAccess == NetworkAccess.Internet;

        // --- Sin internet: intentar caché ---
        if (!hayConexion)
        {
            _logger.LogWarning("Sin conexión a internet. Cargando caché local.");
            return await CargarDesdeCacheAsync(ubicacion, "Sin conexión a internet.");
        }

        // --- Consultar API ---
        List<Farmacia> farmacias;
        try
        {
            farmacias = await _farmaciaProvider.ObtenerFarmaciasAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar API. Fallback a caché.");
            return await CargarDesdeCacheAsync(ubicacion, "No se pudo conectar a la fuente de datos. Mostrando datos guardados.");
        }

        if (farmacias.Count == 0)
        {
            _logger.LogWarning("API retornó lista vacía");
            return await CargarDesdeCacheAsync(ubicacion, "La fuente oficial no entregó datos en este momento.");
        }

        // --- Calcular estado de apertura ---
        var ahora = DateTime.Now;
        foreach (var f in farmacias)
            f.Estado = _aperturaService.Determinar(f, ahora);

        // --- Calcular distancias (sincrónico) ---
        if (ubicacion != null)
            _geoDistanciaService.AsignarDistancias(farmacias, ubicacion);

        // --- Guardar en caché ---
        await _farmaciaRepository.GuardarLoteAsync(farmacias);

        // --- Geocodificar en background (no bloqueante) ---
        if (ubicacion != null)
            _ = GeocodificarFaltantesEnBackgroundAsync(farmacias, ubicacion);

        // --- Recopilar todas con distancia (para slider UI) ---
        var todasConDistancia = ubicacion != null
            ? farmacias
                .Where(f => f.DistanciaKm.HasValue)
                .OrderBy(f => f.Estado == EstadoApertura.Cerrada ? 1 : 0)
                .ThenBy(f => f.DistanciaKm!.Value)
                .ToList()
            : new List<Farmacia>();

        // --- Filtrar por radio progresivo ---
        var resultado = AplicarFiltroRadio(farmacias, ubicacion);

        return BusquedaResultado.Exitoso(resultado, FuenteBusqueda.Api,
            todasConDistancia: todasConDistancia,
            ubicacionUsuario: ubicacion);
    }

    private async Task<BusquedaResultado> CargarDesdeCacheAsync(
        UbicacionUsuario? ubicacion,
        string advertencia)
    {
        var cacheadas = await _farmaciaRepository.ObtenerUltimasAsync();

        if (cacheadas.Count == 0)
            return BusquedaResultado.ConError("Sin datos disponibles. Conéctate a internet e intenta nuevamente.");

        var fechaCache = cacheadas.Max(f => f.FechaConsulta);
        var advertenciaFinal = $"{advertencia} Datos del {fechaCache:dd/MM HH:mm}.";

        // Recalcular estado con horario actual
        var ahora = DateTime.Now;
        foreach (var f in cacheadas)
            f.Estado = _aperturaService.Determinar(f, ahora);

        if (ubicacion != null)
            _geoDistanciaService.AsignarDistancias(cacheadas, ubicacion);

        var todasConDistancia = ubicacion != null
            ? cacheadas
                .Where(f => f.DistanciaKm.HasValue)
                .OrderBy(f => f.Estado == EstadoApertura.Cerrada ? 1 : 0)
                .ThenBy(f => f.DistanciaKm!.Value)
                .ToList()
            : new List<Farmacia>();

        var resultado = AplicarFiltroRadio(cacheadas, ubicacion);
        return BusquedaResultado.Exitoso(resultado, FuenteBusqueda.Cache, advertenciaFinal,
            todasConDistancia, ubicacionUsuario: ubicacion);
    }

    private List<Farmacia> AplicarFiltroRadio(List<Farmacia> farmacias, UbicacionUsuario? ubicacion)
    {
        if (ubicacion == null)
            return farmacias.Take(AppConstants.MaxResultadosLista).ToList();

        // Radio progresivo: 5km → 15km → 50km → 200km
        // Se usa un tope máximo razonable para Chile continental (~200km)
        // para evitar que aparezcan farmacias de territorios insulares (ej: Isla de Pascua)
        foreach (var radio in new[] {
            AppConstants.RadioInicialKm,
            AppConstants.RadioAmpliadoKm,
            AppConstants.RadioExtendidoKm,
            AppConstants.RadioMaximoKm })
        {
            var filtradas = _geoDistanciaService.FiltrarYOrdenarPorRadio(farmacias, ubicacion, radio);
            if (filtradas.Count > 0)
            {
                _logger.LogInformation("Encontradas {Count} farmacias en radio {Radio}km", filtradas.Count, radio);
                return filtradas.Take(AppConstants.MaxResultadosLista).ToList();
            }
        }

        return [];
    }

    private async Task GeocodificarFaltantesEnBackgroundAsync(
        List<Farmacia> farmacias,
        UbicacionUsuario ubicacion)
    {
        // Limitar a 10 farmacias por búsqueda para respetar el rate limit de Nominatim
        // y evitar loops largos que drenen batería si la API retorna muchas sin GPS.
        var sinCoordenadas = farmacias
            .Where(f => !f.TieneCoordenadas)
            .Take(10)
            .ToList();

        if (sinCoordenadas.Count == 0)
            return;

        _logger.LogInformation("Geocodificando {Count} farmacias en background (máx 10)", sinCoordenadas.Count);

        // Timeout de 30 s para todo el lote: 10 farmacias × ~1 s throttle × margen.
        // Evita que el loop corra indefinidamente si una nueva búsqueda ya fue iniciada.
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        foreach (var f in sinCoordenadas)
        {
            if (cts.Token.IsCancellationRequested)
                break;

            try
            {
                var coords = await _geocodingService.GeocodificarAsync(f.Direccion, f.Comuna, cts.Token);
                if (coords.HasValue)
                {
                    f.Latitud = coords.Value.Lat;
                    f.Longitud = coords.Value.Lon;
                    f.DistanciaKm = _geoDistanciaService.CalcularKm(ubicacion, f.Latitud.Value, f.Longitud.Value);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Geocodificación background cancelada por timeout");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No se pudo geocodificar: {Nombre}", f.Nombre);
            }
        }
    }
}
