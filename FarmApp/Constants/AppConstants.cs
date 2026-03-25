namespace FarmApp.Constants;

public static class AppConstants
{
    // API
    public const string MinSalApiUrl =
        "https://midas.minsal.cl/farmacia_v2/WS/getLocalesTurnos.php";

    public const int ApiTimeoutSegundos = 10;

    // Geocodificación (Nominatim / OpenStreetMap — uso permitido con User-Agent correcto)
    public const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/search";
    public const string NominatimUserAgent = "FarmApp/1.0 (hectorariquelmec@gmail.com)";

    // Caché
    public const int CacheDiasMaximos = 2;

    // Búsqueda por radio (km)
    public const double RadioInicialKm = 5.0;
    public const double RadioAmpliadoKm = 15.0;
    public const double RadioExtendidoKm = 50.0;
    public const double RadioMaximoKm = 200.0;
    public const int MaxResultadosLista = 20;

    // Base de datos local
    public const string NombreBaseDatos = "farmapp.db";

    // Preferencias
    public const string PrefRadioKm = "pref_radio_km";
    public const string PrefTemaApp = "pref_tema_app";
    public const string PrefUltimaComuna = "pref_ultima_comuna";
}
