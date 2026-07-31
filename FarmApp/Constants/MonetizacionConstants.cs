namespace FarmApp.Constants;

/// <summary>
/// Identificadores de monetización (AdMob + compras in-app).
///
/// ⚠️ IMPORTANTE ANTES DE PUBLICAR:
/// Los IDs de AdMob de abajo son los IDs DE PRUEBA oficiales de Google.
/// Nunca generan ingresos. Reemplazarlos por los IDs reales de la cuenta
/// AdMob (https://apps.admob.com) antes de generar el AAB de producción.
/// El APPLICATION_ID real también debe actualizarse en
/// Platforms/Android/AndroidManifest.xml
/// (meta-data com.google.android.gms.ads.APPLICATION_ID).
/// </summary>
public static class MonetizacionConstants
{
    // ── Compras in-app (Google Play Billing) ──

    /// <summary>
    /// Producto no consumible que desbloquea FarmApp Pro.
    /// Debe crearse en Play Console → Productos → Productos integrados
    /// con este ID exacto.
    /// </summary>
    public const string ProductoProId = "farmapp_pro";

    /// <summary>Preferencia local: el usuario ya compró FarmApp Pro.</summary>
    public const string PrefEsPro = "pref_es_pro";

    // ── AdMob (IDs DE PRUEBA de Google — reemplazar por los reales) ──

    /// <summary>Unidad de banner de la pantalla de resultados.</summary>
    public const string AdMobBannerResultadosId = "ca-app-pub-3940256099942544/6300978111";
}
