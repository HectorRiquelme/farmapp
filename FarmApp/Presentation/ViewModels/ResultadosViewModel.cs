using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmApp.Domain.Models;

namespace FarmApp.Presentation.ViewModels;

[QueryProperty(nameof(ResultadoBusqueda), "ResultadoBusqueda")]
public partial class ResultadosViewModel : BaseViewModel
{
    // ─────────────────────────────────────────────────────
    //  Datos del resultado
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoAdvertencia))]
    [NotifyPropertyChangedFor(nameof(MostrarAdvertencia))]
    private BusquedaResultado? _resultadoBusqueda;

    // ─────────────────────────────────────────────────────
    //  Estado de selección (centra el mini mapa)
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    private string _farmaciaSeleccionadaEnMapaId = string.Empty;

    // ─────────────────────────────────────────────────────
    //  Slider de radio de búsqueda (1–25 km, por defecto 5)
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RadioTexto))]
    private double _radioKm = 5.0;

    public string RadioTexto => $"{(int)RadioKm} km";

    // ─────────────────────────────────────────────────────
    //  Visibilidad
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _tieneResultados;

    [ObservableProperty]
    private bool _sinResultados;

    /// <summary>
    /// Hay datos con coordenadas → mostrar slider de radio.
    /// </summary>
    [ObservableProperty]
    private bool _tieneDistancias;

    // ─────────────────────────────────────────────────────
    //  Señal para que la Page recargue el mapa
    // ─────────────────────────────────────────────────────

    [ObservableProperty]
    private int _mapaVersion;

    // ─────────────────────────────────────────────────────
    //  Colecciones
    // ─────────────────────────────────────────────────────

    public ObservableCollection<Farmacia> Farmacias { get; } = [];

    /// <summary>Lista plana de todas las farmacias visibles, para el mapa.</summary>
    public List<Farmacia> TodasLasFarmacias => [.. Farmacias];

    public Farmacia? MasCercana => Farmacias.Count > 0 ? Farmacias[0] : null;

    // ─────────────────────────────────────────────────────
    //  Advertencia
    // ─────────────────────────────────────────────────────

    public string TextoAdvertencia => ResultadoBusqueda?.Advertencia ?? string.Empty;
    public bool MostrarAdvertencia => ResultadoBusqueda?.TieneAdvertencia == true;

    // ─────────────────────────────────────────────────────
    //  Cuando llegan los datos (navegación Shell)
    // ─────────────────────────────────────────────────────

    partial void OnResultadoBusquedaChanged(BusquedaResultado? value)
    {
        if (value == null) return;

        TieneDistancias = value.TodasConDistancia.Count > 0;

        // Ajustar el slider al radio efectivo que usó el UseCase
        if (TieneDistancias && value.Farmacias.Count > 0)
        {
            var maxDist = value.Farmacias
                .Where(f => f.DistanciaKm.HasValue)
                .Select(f => f.DistanciaKm!.Value)
                .DefaultIfEmpty(5)
                .Max();
            RadioKm = Math.Min(25, Math.Max(5, Math.Ceiling(maxDist)));
        }

        ActualizarFarmacias(value.Farmacias);
    }

    // ─────────────────────────────────────────────────────
    //  Confirmar radio (slider → re-filtrar lista y mapa)
    // ─────────────────────────────────────────────────────

    [RelayCommand]
    private void ConfirmarRadio()
    {
        var resultado = ResultadoBusqueda;
        if (resultado?.TodasConDistancia.Count > 0)
        {
            var filtradas = resultado.TodasConDistancia
                .Where(f => f.DistanciaKm.HasValue && f.DistanciaKm.Value <= RadioKm)
                .Take(20)
                .ToList();

            if (filtradas.Count > 0)
                ActualizarFarmacias(filtradas);
            // Si el radio es muy pequeño para contener alguna farmacia, la lista no cambia
        }
    }

    // ─────────────────────────────────────────────────────
    //  Actualizar la colección Farmacias
    // ─────────────────────────────────────────────────────

    private void ActualizarFarmacias(List<Farmacia> farmacias)
    {
        FarmaciaSeleccionadaEnMapaId = string.Empty;
        Farmacias.Clear();

        for (int i = 0; i < farmacias.Count; i++)
            farmacias[i].EsMasCercana = (i == 0);

        foreach (var f in farmacias)
            Farmacias.Add(f);

        TieneResultados = Farmacias.Count > 0;
        SinResultados   = !TieneResultados && ResultadoBusqueda != null;

        OnPropertyChanged(nameof(MasCercana));
        MapaVersion++;
    }

    // ─────────────────────────────────────────────────────
    //  Selección de card (centra mapa)
    // ─────────────────────────────────────────────────────

    [RelayCommand]
    private void SeleccionarFarmacia(Farmacia farmacia) =>
        FarmaciaSeleccionadaEnMapaId = farmacia.Id;

    // ─────────────────────────────────────────────────────
    //  Navegar al detalle
    // ─────────────────────────────────────────────────────

    [RelayCommand]
    private async Task VerDetalleFarmaciaAsync(Farmacia farmacia) =>
        await Shell.Current.GoToAsync(nameof(Pages.DetallePage),
            new Dictionary<string, object> { ["Farmacia"] = farmacia });

    [RelayCommand]
    private static async Task VolverAsync() =>
        await Shell.Current.GoToAsync("..");
}
