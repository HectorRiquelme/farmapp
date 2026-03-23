using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FarmApp.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FarmApp.Presentation.ViewModels;

[QueryProperty(nameof(Farmacia), "Farmacia")]
public partial class DetalleFarmaciaViewModel : BaseViewModel
{
    private readonly ILogger<DetalleFarmaciaViewModel> _logger;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneTelefono))]
    [NotifyPropertyChangedFor(nameof(TieneCoordenadas))]
    [NotifyPropertyChangedFor(nameof(FuenteConsultaTexto))]
    private Farmacia? _farmacia;

    public bool TieneTelefono => Farmacia?.TieneTelefono == true;

    public bool TieneCoordenadas => Farmacia?.TieneCoordenadas == true;

    public string FuenteConsultaTexto
    {
        get
        {
            if (Farmacia is null)
                return string.Empty;

            var fuente = string.IsNullOrWhiteSpace(Farmacia.Fuente)
                ? "Fuente no disponible"
                : Farmacia.Fuente;

            return Farmacia.FechaConsulta == default
                ? $"Fuente: {fuente}"
                : $"Fuente: {fuente} · {Farmacia.FechaConsulta:dd/MM HH:mm}";
        }
    }

    public DetalleFarmaciaViewModel(ILogger<DetalleFarmaciaViewModel> logger)
    {
        _logger = logger;
        Titulo = "Detalle";
    }

    [RelayCommand]
    private void LlamarFarmacia()
    {
        if (Farmacia == null || !Farmacia.TieneTelefono)
            return;

        try
        {
            var numero = new string(Farmacia.Telefono.Where(char.IsDigit).ToArray());
            PhoneDialer.Default.Open(numero);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir marcador de teléfono");
        }
    }

    [RelayCommand]
    private async Task NavegarAFarmaciaAsync()
    {
        if (Farmacia == null)
            return;

        try
        {
            if (Farmacia.TieneCoordenadas)
            {
                // Opción 1: Abrir con coordenadas directas
                var mapLocation = new Location(Farmacia.Latitud!.Value, Farmacia.Longitud!.Value);
                var options = new MapLaunchOptions { Name = Farmacia.Nombre };
                await Map.Default.OpenAsync(mapLocation, options);
            }
            else
            {
                // Opción 2: Buscar por dirección en texto
                var placemark = new Placemark
                {
                    FeatureName = Farmacia.Nombre,
                    Thoroughfare = Farmacia.Direccion,
                    Locality = Farmacia.Comuna,
                    CountryName = "Chile"
                };
                await Map.Default.OpenAsync(placemark);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al abrir navegación");
        }
    }

    [RelayCommand]
    private async Task CopiarDireccionAsync()
    {
        if (Farmacia == null)
            return;

        var texto = $"{Farmacia.Nombre}\n{Farmacia.Direccion}, {Farmacia.Comuna}";
        await Clipboard.Default.SetTextAsync(texto);
    }

    [RelayCommand]
    private static async Task VolverAsync() =>
        await Shell.Current.GoToAsync("..");
}
