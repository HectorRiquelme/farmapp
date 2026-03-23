using System.Text.Json;
using FarmApp.Domain.Models;

namespace FarmApp.Presentation.Controls;

/// <summary>
/// Mapa referencial usando Leaflet + OpenStreetMap (CartoDB Dark tiles).
/// Muestra todos los pins de farmacias. Toca un pin para centrar el mapa en él.
/// La navegación paso a paso se hace con el botón "Navegar" de la app (externo).
/// </summary>
public partial class MiniMapView : ContentView
{
    private bool _mapLoaded;
    private List<Farmacia>? _pendingFarmacias;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    public MiniMapView()
    {
        InitializeComponent();
        _ = CargarHtmlAsync();
    }

    private async Task CargarHtmlAsync()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("farmacia_map.html");
            using var reader = new StreamReader(stream);
            var html = await reader.ReadToEndAsync();
            MapWebView.Source = new HtmlWebViewSource { Html = html };
        }
        catch
        {
            // Fallback si no se puede cargar el HTML
            MapWebView.Source = new HtmlWebViewSource
            {
                Html = "<html><body style='background:#0D1117;color:#8B949E;" +
                       "font-family:sans-serif;display:flex;align-items:center;" +
                       "justify-content:center;height:100%;font-size:14px'>" +
                       "Mapa no disponible</body></html>"
            };
            LoadingIndicator.IsVisible = false;
        }
    }

    /// <summary>
    /// Intercepta la señal "farmapp://ready" que el HTML envía cuando Leaflet está listo.
    /// </summary>
    private void OnMapNavigating(object sender, WebNavigatingEventArgs e)
    {
        if (e.Url.StartsWith("farmapp://ready"))
        {
            e.Cancel = true;
            OnMapReady();
        }
    }

    /// <summary>
    /// Fallback: si la señal JS no llegó, intentar después de que la página cargue.
    /// </summary>
    private void OnMapNavigated(object sender, WebNavigatedEventArgs e)
    {
        if (e.Result != WebNavigationResult.Success || _mapLoaded) return;

        // Fallback: esperar que Leaflet termine de inicializar (~1.2s)
        Task.Delay(1200).ContinueWith(_ =>
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!_mapLoaded) OnMapReady();
            }));
    }

    private void OnMapReady()
    {
        _mapLoaded = true;
        LoadingIndicator.IsRunning = false;
        LoadingIndicator.IsVisible = false;

        if (_pendingFarmacias != null)
        {
            var f = _pendingFarmacias;
            _pendingFarmacias = null;
            _ = EnviarFarmaciasAlMapaAsync(f);
        }
    }

    /// <summary>
    /// Carga o recarga todos los pins de farmacias en el mapa.
    /// </summary>
    public void LoadFarmacias(List<Farmacia> farmacias)
    {
        if (!_mapLoaded)
        {
            _pendingFarmacias = farmacias;
            return;
        }
        _ = EnviarFarmaciasAlMapaAsync(farmacias);
    }

    /// <summary>
    /// Centra el mapa en la farmacia con el id dado y abre su popup.
    /// Llamado desde el exterior cuando el usuario toca una tarjeta de la lista.
    /// </summary>
    public void CentrarEn(string farmaciaId)
    {
        if (!_mapLoaded || string.IsNullOrEmpty(farmaciaId)) return;
        var id = farmaciaId.Replace("'", "\\'").Replace("\\", "\\\\");
        _ = MapWebView.EvaluateJavaScriptAsync($"centrarEn('{id}')");
    }

    private async Task EnviarFarmaciasAlMapaAsync(List<Farmacia> farmacias)
    {
        // Construir array de datos para el mapa
        var data = farmacias.Select(f => new
        {
            id     = f.Id,
            nombre = f.Nombre,
            lat    = f.TieneCoordenadas ? f.Latitud    : (double?)null,
            lon    = f.TieneCoordenadas ? f.Longitud   : (double?)null,
            estado = f.Estado.ToString(),
            tipo   = f.Tipo.ToString()
        }).ToList();

        var json = JsonSerializer.Serialize(data, JsonOpts);

        try
        {
            await MapWebView.EvaluateJavaScriptAsync($"loadFarmacias({json})");

            // Mostrar hint solo si hay pins cargados
            if (data.Any(d => d.lat != null))
                HintLabel.IsVisible = true;
        }
        catch
        {
            // Silencioso si el mapa no está disponible
        }
    }
}
