namespace FarmApp.Domain.Models;

/// <summary>Estado final de una operación de compra o restauración de FarmApp Pro.</summary>
public enum EstadoCompraPro
{
    /// <summary>La compra se completó y quedó reconocida.</summary>
    Comprada,

    /// <summary>El usuario ya tenía FarmApp Pro activo.</summary>
    YaEraPro,

    /// <summary>Pago pendiente de aprobación (ej: pago en efectivo).</summary>
    Pendiente,

    /// <summary>El usuario canceló el flujo de compra.</summary>
    Cancelada,

    /// <summary>No se encontraron compras previas al restaurar.</summary>
    SinCompras,

    /// <summary>Error de conexión o de Google Play.</summary>
    Error
}

/// <summary>Resultado de una operación de compra o restauración de FarmApp Pro.</summary>
public sealed record ResultadoCompraPro(EstadoCompraPro Estado, string? Mensaje = null)
{
    public bool EsExitoso => Estado is EstadoCompraPro.Comprada or EstadoCompraPro.YaEraPro;
}
