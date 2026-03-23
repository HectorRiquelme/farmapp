using FarmApp.Domain.Models;

namespace FarmApp.Presentation.Controls;

public partial class EstadoBadge : ContentView
{
    public static readonly BindableProperty EstadoProperty =
        BindableProperty.Create(nameof(Estado), typeof(EstadoApertura), typeof(EstadoBadge),
            EstadoApertura.SinDatos, propertyChanged: OnEstadoChanged);

    public static readonly BindableProperty TipoProperty =
        BindableProperty.Create(nameof(Tipo), typeof(TipoFarmacia), typeof(EstadoBadge),
            TipoFarmacia.NoDefinido, propertyChanged: OnEstadoChanged);

    public EstadoApertura Estado
    {
        get => (EstadoApertura)GetValue(EstadoProperty);
        set => SetValue(EstadoProperty, value);
    }

    public TipoFarmacia Tipo
    {
        get => (TipoFarmacia)GetValue(TipoProperty);
        set => SetValue(TipoProperty, value);
    }

    public EstadoBadge()
    {
        InitializeComponent();
        Actualizar();
    }

    private static void OnEstadoChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is EstadoBadge badge)
            badge.Actualizar();
    }

    private void Actualizar()
    {
        var (texto, color) = Estado switch
        {
            EstadoApertura.AbiertaAhora => ("● Abierta ahora", Color.FromArgb("#22C55E")),
            EstadoApertura.PosiblementeAbierta => ("◐ Posiblemente abierta", Color.FromArgb("#F59E0B")),
            EstadoApertura.HorarioNoConfirmado => ("? Horario no confirmado", Color.FromArgb("#6B7280")),
            EstadoApertura.Cerrada => ("✕ Cerrada", Color.FromArgb("#4B5563")),
            _ => ("— Sin datos", Color.FromArgb("#374151"))
        };

        // Si es urgencia, añadir indicador
        if (Tipo == TipoFarmacia.Urgencia && Estado == EstadoApertura.AbiertaAhora)
        {
            texto = "⚡ Urgencia · Abierta";
            color = Color.FromArgb("#3B82F6");
        }

        BadgeLabel.Text = texto;
        BadgeFrame.BackgroundColor = color.WithAlpha(0.2f);
        BadgeFrame.BorderColor = color;
        BadgeLabel.TextColor = color;
    }
}
