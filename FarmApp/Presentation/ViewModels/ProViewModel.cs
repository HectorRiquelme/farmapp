using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;

namespace FarmApp.Presentation.ViewModels;

public partial class ProViewModel : BaseViewModel
{
    private readonly IProService _proService;

    // ─────────────────────────────────────────────────────
    //  Estado Pro
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoEsPro))]
    private bool _esPro;

    public bool NoEsPro => !EsPro;

    [ObservableProperty]
    private string _precioTexto = "…";

    // ─────────────────────────────────────────────────────
    //  Mensajes al usuario
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    private string _mensaje = string.Empty;

    [ObservableProperty]
    private bool _tieneMensaje;

    // ─────────────────────────────────────────────────────
    //  Ajuste Pro: tema de la app
    // ─────────────────────────────────────────────────────

    public List<string> TemasDisponibles { get; } = ["Sistema", "Claro", "Oscuro"];

    [ObservableProperty]
    private string _temaSeleccionado;

    public ProViewModel(IProService proService)
    {
        _proService = proService;
        Titulo = "FarmApp Pro";
        _esPro = proService.EsPro;

        _temaSeleccionado = Preferences.Default.Get(AppConstants.PrefTemaApp, "sistema") switch
        {
            "claro" => "Claro",
            "oscuro" => "Oscuro",
            _ => "Sistema"
        };
    }

    /// <summary>Carga el precio localizado desde Google Play al abrir la página.</summary>
    public async Task InicializarAsync()
    {
        EsPro = _proService.EsPro;

        if (!EsPro && PrecioTexto == "…")
        {
            var precio = await _proService.ObtenerPrecioLocalizadoAsync();
            PrecioTexto = precio ?? "Precio disponible en Google Play";
        }
    }

    // ─────────────────────────────────────────────────────
    //  Comprar / Restaurar
    // ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task ComprarAsync()
    {
        if (EstaCargando) return;

        EstaCargando = true;
        TieneMensaje = false;
        try
        {
            var resultado = await _proService.ComprarProAsync();
            ProcesarResultado(resultado, esCompra: true);
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand]
    private async Task RestaurarAsync()
    {
        if (EstaCargando) return;

        EstaCargando = true;
        TieneMensaje = false;
        try
        {
            var resultado = await _proService.RestaurarComprasAsync();
            ProcesarResultado(resultado, esCompra: false);
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private void ProcesarResultado(ResultadoCompraPro resultado, bool esCompra)
    {
        EsPro = _proService.EsPro;

        Mensaje = resultado.Estado switch
        {
            EstadoCompraPro.Comprada => "¡Listo! FarmApp Pro está activo. Gracias por apoyar la app 💚",
            EstadoCompraPro.YaEraPro => "FarmApp Pro ya estaba activo en tu cuenta.",
            EstadoCompraPro.Pendiente => resultado.Mensaje ?? "Pago pendiente de confirmación.",
            EstadoCompraPro.Cancelada => esCompra ? "Compra cancelada. Puedes intentarlo cuando quieras." : string.Empty,
            EstadoCompraPro.SinCompras => resultado.Mensaje ?? "No encontramos compras previas.",
            _ => resultado.Mensaje ?? "Ocurrió un error. Intenta nuevamente."
        };

        TieneMensaje = !string.IsNullOrEmpty(Mensaje);
    }

    // ─────────────────────────────────────────────────────
    //  Tema (se aplica al vuelo y se persiste)
    // ─────────────────────────────────────────────────────

    partial void OnTemaSeleccionadoChanged(string value)
    {
        var clave = value switch
        {
            "Claro" => "claro",
            "Oscuro" => "oscuro",
            _ => "sistema"
        };

        Preferences.Default.Set(AppConstants.PrefTemaApp, clave);

        if (Microsoft.Maui.Controls.Application.Current is { } app)
            app.UserAppTheme = clave switch
            {
                "claro" => AppTheme.Light,
                "oscuro" => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
    }
}
