using System.Windows.Input;
using FarmApp.Domain.Models;

namespace FarmApp.Presentation.Controls;

public partial class FarmaciaCardCompacta : ContentView
{
    // ─── Color fijo (igual en ambos temas) ────────────────────────────────
    private static readonly Color ColorSeleccionado = Color.FromArgb("#22C55E");

    // ─── Colores adaptativos según tema activo ────────────────────────────
    private static Color BordeNormal =>
        Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Light
            ? Color.FromArgb("#CBD5E1")  // separador light
            : Color.FromArgb("#30363D"); // separador dark

    private static Color BordeMasCercana =>
        Microsoft.Maui.Controls.Application.Current?.RequestedTheme == AppTheme.Light
            ? Color.FromArgb("#16A34A")  // verde visible sobre blanco
            : Color.FromArgb("#166534"); // verde oscuro sobre dark

    private bool _isSelected;

    // ─── Bindable Properties ──────────────────────────────────────────────

    public static readonly BindableProperty FarmaciaProperty =
        BindableProperty.Create(nameof(Farmacia), typeof(Farmacia), typeof(FarmaciaCardCompacta),
            null, propertyChanged: OnFarmaciaChanged);

    public static readonly BindableProperty SeleccionarCommandProperty =
        BindableProperty.Create(nameof(SeleccionarCommand), typeof(ICommand), typeof(FarmaciaCardCompacta));

    public static readonly BindableProperty VerDetalleCommandProperty =
        BindableProperty.Create(nameof(VerDetalleCommand), typeof(ICommand), typeof(FarmaciaCardCompacta));

    public Farmacia? Farmacia
    {
        get => (Farmacia?)GetValue(FarmaciaProperty);
        set => SetValue(FarmaciaProperty, value);
    }

    public ICommand? SeleccionarCommand
    {
        get => (ICommand?)GetValue(SeleccionarCommandProperty);
        set => SetValue(SeleccionarCommandProperty, value);
    }

    public ICommand? VerDetalleCommand
    {
        get => (ICommand?)GetValue(VerDetalleCommandProperty);
        set => SetValue(VerDetalleCommandProperty, value);
    }

    public bool EsMasCercana { get; private set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            ActualizarBorde();
        }
    }

    // ─── Constructor ──────────────────────────────────────────────────────

    public FarmaciaCardCompacta()
    {
        InitializeComponent();

        var tap = new TapGestureRecognizer();
        tap.Tapped += OnCardTapped;
        CardFrame.GestureRecognizers.Add(tap);

        BtnVerDetalle.Clicked += OnVerDetalleClicked;
        BtnLlamar.Clicked     += (_, _) => LlamarFarmacia();
        BtnNavegar.Clicked    += (_, _) => _ = NavegarAsync();
    }

    // ─── Ciclo de vida: suscribir/desuscribir al cambio de tema ──────────

    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        base.OnHandlerChanging(args);

        var app = Microsoft.Maui.Controls.Application.Current;
        if (args.OldHandler != null && app != null)
            app.RequestedThemeChanged -= OnTemaChanged;

        if (args.NewHandler != null && app != null)
            app.RequestedThemeChanged += OnTemaChanged;
    }

    private void OnTemaChanged(object? sender, AppThemeChangedEventArgs e) =>
        ActualizarBorde();

    // ─── Gestos ───────────────────────────────────────────────────────────

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (Farmacia != null && SeleccionarCommand?.CanExecute(Farmacia) == true)
            SeleccionarCommand.Execute(Farmacia);
    }

    private void OnVerDetalleClicked(object? sender, EventArgs e)
    {
        if (Farmacia != null && VerDetalleCommand?.CanExecute(Farmacia) == true)
            VerDetalleCommand.Execute(Farmacia);
    }

    // ─── Visual ───────────────────────────────────────────────────────────

    private void ActualizarBorde()
    {
        if (_isSelected)
        {
            CardFrame.BorderColor = ColorSeleccionado;
            CardFrame.HasShadow   = true;
        }
        else if (EsMasCercana)
        {
            CardFrame.BorderColor = BordeMasCercana;
            CardFrame.HasShadow   = false;
        }
        else
        {
            CardFrame.BorderColor = BordeNormal;
            CardFrame.HasShadow   = false;
        }
    }

    // ─── Datos ────────────────────────────────────────────────────────────

    private static void OnFarmaciaChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FarmaciaCardCompacta card)
        {
            card.EsMasCercana = newValue is Farmacia f && f.EsMasCercana;
            card.ActualizarUI();
        }
    }

    private void ActualizarUI()
    {
        if (Farmacia == null) return;

        LabelNombre.Text    = Farmacia.Nombre;
        LabelDireccion.Text = $"{Farmacia.Direccion}, {Farmacia.Comuna}";
        LabelHorario.Text   = string.IsNullOrEmpty(Farmacia.HorarioTexto)
            ? "Horario no disponible"
            : Farmacia.HorarioTexto;

        BadgeCard.Estado = Farmacia.Estado;
        BadgeCard.Tipo   = Farmacia.Tipo;

        if (Farmacia.DistanciaKm.HasValue)
        {
            LabelDistancia.Text      = Farmacia.DistanciaTexto;
            LabelDistancia.TextColor = Farmacia.Estado == EstadoApertura.AbiertaAhora
                ? Color.FromArgb("#22C55E")
                : Color.FromArgb("#8B949E");
        }
        else
        {
            LabelDistancia.Text = string.Empty;
        }

        BtnLlamar.IsVisible       = Farmacia.TieneTelefono;
        LabelMasCercana.IsVisible = EsMasCercana;

        ActualizarBorde();
    }

    // ─── Llamar ───────────────────────────────────────────────────────────

    private void LlamarFarmacia()
    {
        if (Farmacia?.TieneTelefono != true) return;
        try
        {
            var numero = new string(Farmacia.Telefono.Where(char.IsDigit).ToArray());
            PhoneDialer.Default.Open(numero);
        }
        catch { /* ignorar si el dispositivo no soporta llamadas */ }
    }

    // ─── Navegar en mapa externo ──────────────────────────────────────────

    private async Task NavegarAsync()
    {
        if (Farmacia == null) return;
        try
        {
            if (Farmacia.TieneCoordenadas)
            {
                var location = new Location(Farmacia.Latitud!.Value, Farmacia.Longitud!.Value);
                await Map.Default.OpenAsync(location, new MapLaunchOptions { Name = Farmacia.Nombre });
            }
            else
            {
                var placemark = new Placemark
                {
                    FeatureName  = Farmacia.Nombre,
                    Thoroughfare = Farmacia.Direccion,
                    Locality     = Farmacia.Comuna,
                    CountryName  = "Chile"
                };
                await Map.Default.OpenAsync(placemark);
            }
        }
        catch { /* ignorar si no hay app de mapas */ }
    }
}
