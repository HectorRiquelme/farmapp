using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Models;
using Microsoft.Extensions.Logging;
using Plugin.InAppBilling;

namespace FarmApp.Infrastructure.Compras;

/// <summary>
/// Implementación de FarmApp Pro sobre Google Play Billing (Plugin.InAppBilling).
/// Sin backend propio: la titularidad se consulta a Google Play y se cachea en
/// Preferences para lectura síncrona (gating de anuncios y features Pro).
/// </summary>
public class ComprasService : IProService
{
    // Serializa las operaciones de billing: Google Play no tolera flujos concurrentes
    private static readonly SemaphoreSlim _mutex = new(1, 1);

    private readonly ILogger<ComprasService> _logger;

    public event EventHandler<bool>? EsProCambiado;

    public ComprasService(ILogger<ComprasService> logger)
    {
        _logger = logger;
    }

    public bool EsPro => Preferences.Default.Get(MonetizacionConstants.PrefEsPro, false);

    // ─────────────────────────────────────────────────────
    //  Compra
    // ─────────────────────────────────────────────────────

    public async Task<ResultadoCompraPro> ComprarProAsync()
    {
        if (EsPro)
            return new ResultadoCompraPro(EstadoCompraPro.YaEraPro);

        await _mutex.WaitAsync();
        var billing = CrossInAppBilling.Current;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            if (!await billing.ConnectAsync(cancellationToken: cts.Token))
                return new ResultadoCompraPro(EstadoCompraPro.Error,
                    "No se pudo conectar con Google Play. Revisa tu conexión.");

            var compra = await billing.PurchaseAsync(
                MonetizacionConstants.ProductoProId, ItemType.InAppPurchase);

            if (compra == null)
                return new ResultadoCompraPro(EstadoCompraPro.Cancelada);

            if (compra.State == PurchaseState.Purchased)
            {
                await ReconocerCompraAsync(billing, compra);
                EstablecerEsPro(true);
                return new ResultadoCompraPro(EstadoCompraPro.Comprada);
            }

            if (compra.State == PurchaseState.PaymentPending)
                return new ResultadoCompraPro(EstadoCompraPro.Pendiente,
                    "Tu pago quedó pendiente. Pro se activará al confirmarse (usa \"Restaurar compras\").");

            return new ResultadoCompraPro(EstadoCompraPro.Cancelada);
        }
        catch (InAppBillingPurchaseException ex)
        {
            return MapearErrorDeCompra(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al comprar FarmApp Pro");
            return new ResultadoCompraPro(EstadoCompraPro.Error,
                "Ocurrió un error inesperado. Intenta nuevamente.");
        }
        finally
        {
            await DesconectarSeguroAsync(billing);
            _mutex.Release();
        }
    }

    // ─────────────────────────────────────────────────────
    //  Restauración (reinstalación o cambio de dispositivo)
    // ─────────────────────────────────────────────────────

    public async Task<ResultadoCompraPro> RestaurarComprasAsync()
    {
        await _mutex.WaitAsync();
        var billing = CrossInAppBilling.Current;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            if (!await billing.ConnectAsync(cancellationToken: cts.Token))
                return new ResultadoCompraPro(EstadoCompraPro.Error,
                    "No se pudo conectar con Google Play. Revisa tu conexión.");

            var compras = await billing.GetPurchasesAsync(ItemType.InAppPurchase, cts.Token);
            var compraPro = compras?.FirstOrDefault(EsCompraProActiva);

            if (compraPro == null)
            {
                EstablecerEsPro(false);
                return new ResultadoCompraPro(EstadoCompraPro.SinCompras,
                    "No encontramos compras de FarmApp Pro en esta cuenta de Google.");
            }

            await ReconocerCompraAsync(billing, compraPro);
            var yaEra = EsPro;
            EstablecerEsPro(true);
            return new ResultadoCompraPro(yaEra ? EstadoCompraPro.YaEraPro : EstadoCompraPro.Comprada);
        }
        catch (InAppBillingPurchaseException ex)
        {
            return MapearErrorDeCompra(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al restaurar compras");
            return new ResultadoCompraPro(EstadoCompraPro.Error,
                "Ocurrió un error inesperado. Intenta nuevamente.");
        }
        finally
        {
            await DesconectarSeguroAsync(billing);
            _mutex.Release();
        }
    }

    // ─────────────────────────────────────────────────────
    //  Precio localizado (lo entrega Google Play en CLP)
    // ─────────────────────────────────────────────────────

    public async Task<string?> ObtenerPrecioLocalizadoAsync()
    {
        await _mutex.WaitAsync();
        var billing = CrossInAppBilling.Current;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            if (!await billing.ConnectAsync(cancellationToken: cts.Token))
                return null;

            var productos = await billing.GetProductInfoAsync(
                ItemType.InAppPurchase, [MonetizacionConstants.ProductoProId], cts.Token);

            return productos?.FirstOrDefault()?.LocalizedPrice;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener el precio de FarmApp Pro");
            return null;
        }
        finally
        {
            await DesconectarSeguroAsync(billing);
            _mutex.Release();
        }
    }

    // ─────────────────────────────────────────────────────
    //  Revalidación silenciosa al arrancar (detecta reembolsos)
    // ─────────────────────────────────────────────────────

    public async Task RevalidarSilenciosoAsync()
    {
        // Solo revalida cuando Pro está activo; el usuario libre no genera tráfico de billing
        if (!EsPro) return;

        await _mutex.WaitAsync();
        var billing = CrossInAppBilling.Current;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            if (!await billing.ConnectAsync(cancellationToken: cts.Token))
                return; // sin conexión: se conserva el estado local

            var compras = await billing.GetPurchasesAsync(ItemType.InAppPurchase, cts.Token);
            var sigueActiva = compras?.Any(EsCompraProActiva) == true;

            if (!sigueActiva)
                EstablecerEsPro(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Revalidación silenciosa de FarmApp Pro omitida");
        }
        finally
        {
            await DesconectarSeguroAsync(billing);
            _mutex.Release();
        }
    }

    // ─────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────

    private static bool EsCompraProActiva(InAppBillingPurchase compra) =>
        compra.ProductId == MonetizacionConstants.ProductoProId &&
        (compra.State == PurchaseState.Purchased || compra.State == PurchaseState.Restored);

    /// <summary>
    /// Reconoce (acknowledge) la compra ante Google Play. Sin este paso,
    /// Google reembolsa automáticamente la compra a los 3 días.
    /// </summary>
    private async Task ReconocerCompraAsync(IInAppBilling billing, InAppBillingPurchase compra)
    {
        if (compra.IsAcknowledged == true || string.IsNullOrEmpty(compra.TransactionIdentifier))
            return;

        try
        {
            await billing.FinalizePurchaseAsync([compra.TransactionIdentifier]);
        }
        catch (Exception ex)
        {
            // Si falla, la próxima restauración lo reintenta (dentro de la ventana de 3 días)
            _logger.LogWarning(ex, "No se pudo reconocer la compra; se reintentará al restaurar");
        }
    }

    private void EstablecerEsPro(bool valor)
    {
        if (EsPro == valor) return;

        Preferences.Default.Set(MonetizacionConstants.PrefEsPro, valor);

        var handler = EsProCambiado;
        if (handler != null)
            MainThread.BeginInvokeOnMainThread(() => handler.Invoke(this, valor));
    }

    private ResultadoCompraPro MapearErrorDeCompra(InAppBillingPurchaseException ex)
    {
        _logger.LogWarning(ex, "Error de Google Play: {Error}", ex.PurchaseError);

        return ex.PurchaseError switch
        {
            PurchaseError.UserCancelled => new ResultadoCompraPro(EstadoCompraPro.Cancelada),

            // Ya la tenía comprada (ej: reinstalación) → activar directamente
            PurchaseError.AlreadyOwned => ActivarPorYaComprado(),

            PurchaseError.BillingUnavailable or PurchaseError.ServiceUnavailable =>
                new ResultadoCompraPro(EstadoCompraPro.Error,
                    "Google Play no está disponible en este momento. Intenta más tarde."),

            PurchaseError.ItemUnavailable =>
                new ResultadoCompraPro(EstadoCompraPro.Error,
                    "El producto no está disponible. Verifica que la app esté instalada desde Google Play."),

            _ => new ResultadoCompraPro(EstadoCompraPro.Error,
                "No se pudo completar la compra. Intenta nuevamente.")
        };
    }

    private ResultadoCompraPro ActivarPorYaComprado()
    {
        EstablecerEsPro(true);
        return new ResultadoCompraPro(EstadoCompraPro.YaEraPro);
    }

    private static async Task DesconectarSeguroAsync(IInAppBilling billing)
    {
        try
        {
            await billing.DisconnectAsync();
        }
        catch
        {
            // La desconexión nunca debe romper el flujo principal
        }
    }
}
