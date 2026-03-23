# PROJECT_CONTEXT.md — FarmApp

> Contexto completo del proyecto. Última actualización: 2026-03-22.

## 1. Identidad

- **Nombre:** FarmApp
- **App ID:** `cl.farmapp.farmaciaabierta`
- **Versión:** 1.0 (ApplicationVersion=1)
- **Tipo:** App móvil multiplataforma (.NET MAUI 8)
- **Target prioritario:** Android (API 21+, arm64)
- **Objetivo:** Encontrar la farmacia de turno nocturno más cercana en Chile usando datos oficiales MINSAL

## 2. Estado actual (verificado 2026-03-22)

### Completado
- [x] UI completa: HomePage, ResultadosPage, DetallePage
- [x] Mapa Leaflet interactivo embebido en WebView (tiles CartoDB Dark)
- [x] Consumo API MIDAS/MINSAL con fallback a caché SQLite
- [x] Geocodificación background con Nominatim OSM (caché en SQLite)
- [x] Cálculo de distancias Haversine + filtro radio progresivo (5→15→50→200 km)
- [x] Sistema de colores semántico dark/light con AppThemeBinding
- [x] Tarjetas con borde reactivo al tema del sistema
- [x] Conexión SQLite compartida (DatabaseConnection singleton) — sin contención
- [x] CancellationToken + límite Take(10) en geocodificación background
- [x] Leaflet bundled offline (CSS + JS en Resources/Raw/)
- [x] iOS Info.plist con NSLocationWhenInUseUsageDescription
- [x] LimpiarRegistrosViejosAsync invocado en BuscarFarmaciasUseCase
- [x] FarmaciaCardDestacada eliminada (no existe en el proyecto)
- [x] Build Android debug y release firmado: 0 errores, 0 warnings

### Pendiente
- [ ] Validación en dispositivo físico (Samsung S23 Ultra / Xiaomi MIUI)
- [ ] Configuración Play Console (Data Safety, capturas, descripción)
- [ ] Política de privacidad (requerida por Google Play)
- [ ] AAB (Android App Bundle) para Play Store (actualmente se genera APK)

## 3. Stack tecnológico

| Componente | Tecnología | Versión |
|------------|-----------|---------|
| Framework | .NET MAUI | 8.0 |
| MVVM | CommunityToolkit.Mvvm | 8.3.2 |
| UI toolkit | CommunityToolkit.Maui | 9.1.1 |
| HTTP | Microsoft.Extensions.Http | 8.0.0 |
| Base de datos | sqlite-net-pcl | 1.9.172 |
| SQLite nativo | SQLitePCLRaw.bundle_green | 2.1.10 |
| Mapa | Leaflet.js | 1.9.4 (bundled) |
| Tiles | CartoDB Dark Matter | CDN online |
| Geocodificación | Nominatim OSM | API pública |
| API datos | MIDAS/MINSAL | API pública |

**Sin backend propio.** La app consume directamente las APIs públicas.

## 4. Arquitectura

Clean Architecture en 4 capas con MVVM en presentación:

```
Presentation  ←  XAML + ViewModels + Controls + Converters
     ↓
Application   ←  BuscarFarmaciasUseCase (único caso de uso)
     ↓
Domain        ←  Farmacia, BusquedaResultado, UbicacionUsuario, Enums
                  IFarmaciaProvider, IFarmaciaRepository, IGeoCacheRepository, ILocationService
                  AperturaService, GeoDistanciaService
     ↑
Infrastructure ← MinSalApiService, GeocodingService, ApiNormalizer
                  DatabaseConnection, FarmaciaRepository, GeoCacheRepository
                  MauiLocationService
```

## 5. Estructura del proyecto

```
FarmApp/
├── FarmApp.csproj
├── MauiProgram.cs                        ← DI container
├── App.xaml / App.xaml.cs                ← Bootstrap, UserAppTheme = Unspecified
├── AppShell.xaml / .cs                   ← Shell, ruta raíz = HomePage
├── Constants/
│   └── AppConstants.cs                   ← URLs, timeouts, radios, nombre DB
├── Domain/
│   ├── Models/
│   │   ├── Farmacia.cs                   ← Entidad principal (SQLite)
│   │   ├── BusquedaResultado.cs          ← Resultado de búsqueda
│   │   ├── UbicacionUsuario.cs           ← Record (Lat, Lon)
│   │   └── Enums.cs                      ← TipoFarmacia, EstadoApertura, FuenteBusqueda
│   ├── Interfaces/
│   │   ├── IFarmaciaProvider.cs
│   │   ├── IFarmaciaRepository.cs
│   │   ├── IGeoCacheRepository.cs
│   │   └── ILocationService.cs
│   └── Services/
│       ├── AperturaService.cs            ← Estado apertura por horario
│       └── GeoDistanciaService.cs        ← Haversine + filtro por radio
├── Application/
│   └── BuscarFarmaciasUseCase.cs         ← Orquestación completa
├── Infrastructure/
│   ├── Api/
│   │   ├── MinSalApiService.cs           ← HTTP MIDAS (IFarmaciaProvider)
│   │   ├── GeocodingService.cs           ← Nominatim + throttle + GeoCache
│   │   ├── ApiNormalizer.cs              ← DTO → Farmacia
│   │   └── Dtos/MidasFarmaciaDto.cs      ← Mapeo JSON API
│   ├── Cache/
│   │   ├── DatabaseConnection.cs         ← SQLite singleton compartido
│   │   ├── FarmaciaRepository.cs         ← SQLite farmacias
│   │   └── GeoCacheRepository.cs         ← SQLite coordenadas + clase GeoCache
│   └── Location/
│       └── MauiLocationService.cs        ← GPS vía MAUI Geolocation
├── Presentation/
│   ├── Pages/
│   │   ├── HomePage.xaml / .cs           ← Pantalla inicio con logo y botón buscar
│   │   ├── ResultadosPage.xaml / .cs     ← Lista + slider radio + miniMapa
│   │   └── DetallePage.xaml / .cs        ← Detalle farmacia + llamar/navegar
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs              ← EstaCargando, Titulo, NoEstaCargando
│   │   ├── HomeViewModel.cs              ← Búsqueda + permisos GPS
│   │   ├── ResultadosViewModel.cs        ← Filtrado + control mapa
│   │   └── DetalleFarmaciaViewModel.cs   ← Detalle + acciones
│   ├── Controls/
│   │   ├── FarmaciaCardCompacta.xaml/.cs  ← Tarjeta unificada
│   │   ├── EstadoBadge.xaml / .cs        ← Badge estado con color
│   │   └── MiniMapView.xaml / .cs        ← WebView Leaflet + JS bridge
│   └── Converters/
│       └── InvertBoolConverter.cs
├── Properties/
│   └── launchSettings.json               ← Configuración de debug
├── Platforms/
│   ├── Android/AndroidManifest.xml       ← ACCESS_NETWORK_STATE, INTERNET, FINE_LOCATION, COARSE_LOCATION, CALL_PHONE
│   └── iOS/Info.plist                    ← NSLocationWhenInUseUsageDescription
└── Resources/
    ├── Raw/
    │   ├── farmacia_map.html             ← HTML Leaflet (offline)
    │   ├── leaflet.min.js                ← Leaflet bundled
    │   └── leaflet.min.css               ← Leaflet CSS bundled
    └── Styles/
        ├── Colors.xaml                   ← Paleta semántica dark/light
        └── Styles.xaml                   ← Estilos base MAUI
```

## 6. Modelo de dominio

### Farmacia (entidad SQLite)
- **Identificación:** Id (PrimaryKey), Nombre, Direccion, Comuna, Region
- **Ubicación:** Latitud?, Longitud?, TieneCoordenadas ([Ignore] computed)
- **Contacto:** Telefono, TieneTelefono ([Ignore] computed)
- **Horario:** HorarioTexto, AperturaMinutos, CierreMinutos → Apertura/Cierre (TimeSpan, [Ignore] computed)
- **Clasificación:** Tipo (TipoFarmacia), Fuente, FechaConsulta
- **Estado:** Estado (EstadoApertura, persistido en SQLite), Observaciones
- **Runtime-only ([Ignore]):** DistanciaKm?, DistanciaTexto (computed), EsMasCercana

### Enums
- **TipoFarmacia:** Turno, Urgencia, NoDefinido
- **EstadoApertura:** AbiertaAhora, PosiblementeAbierta, HorarioNoConfirmado, Cerrada, SinDatos
- **FuenteBusqueda:** Api, Cache, SinResultados

## 7. Configuración (AppConstants.cs)

| Constante | Valor |
|-----------|-------|
| MinSalApiUrl | `https://midas.minsal.cl/farmacia_v2/WS/getLocalesTurnos.php` |
| ApiTimeoutSegundos | 10 |
| NominatimBaseUrl | `https://nominatim.openstreetmap.org/search` |
| NominatimUserAgent | `FarmApp/1.0 (farmapp@ejemplo.cl)` |
| CacheDiasMaximos | 2 |
| RadioInicialKm | 5.0 |
| RadioAmpliadoKm | 15.0 |
| RadioExtendidoKm | 50.0 |
| RadioMaximoKm | 200.0 |
| MaxResultadosLista | 20 |
| NombreBaseDatos | `farmapp.db` |
| PrefRadioKm | `pref_radio_km` (definida, sin uso actual) |
| PrefTemaApp | `pref_tema_app` (definida, sin uso actual) |
| PrefUltimaComuna | `pref_ultima_comuna` (definida, sin uso actual) |

## 8. Flujo de búsqueda completo

1. Usuario toca "Buscar ahora" → `HomeViewModel.BuscarFarmaciasAsync()`
2. `MauiLocationService` solicita permiso GPS + obtiene ubicación (timeout 15s)
3. `BuscarFarmaciasUseCase.EjecutarAsync(ubicacion)`:
   - Limpieza preventiva de registros viejos (fire-and-forget)
   - Verifica `Connectivity.NetworkAccess`
   - Si offline → carga caché SQLite + advertencia
   - Si online → `MinSalApiService` GET MIDAS → `ApiNormalizer` transforma DTOs
   - `AperturaService.Determinar()` asigna estado a cada farmacia
   - `GeoDistanciaService.AsignarDistancias()` calcula km (Haversine)
   - Persiste en SQLite (reemplaza todo el lote)
   - Geocodifica faltantes en background (máx 10, timeout 30s, CancellationToken)
   - Filtra por radio progresivo (5→15→50→200 km), top 20
4. Shell navega a `ResultadosPage` con `BusquedaResultado`
5. `ResultadosViewModel` popula lista, marca la más cercana, carga mapa
6. `MiniMapView` invoca JS: `loadFarmacias(json)` → pines en Leaflet
7. Tap en tarjeta → `centrarEn(id)` en JS + scroll en lista
8. Tap en "Ver detalle" → navega a `DetallePage`

## 9. Paleta de colores (Colors.xaml)

### Base
| Token | Dark | Light | Uso |
|-------|------|-------|-----|
| ColorBackground | #0D1117 | #F1F5F9 | Background principal |
| ColorSurface | #161B22 | #FFFFFF | Cards, contenedores |
| ColorSeparador | #30363D | #CBD5E1 | Bordes y líneas |

### Texto
| Token | Dark | Light | Uso |
|-------|------|-------|-----|
| ColorTextoPrimario | #F0F6FC | #0F172A | Títulos, contenido |
| ColorTextoSecundario | #8B949E | #475569 | Subtítulos, meta |
| ColorTextoSutil | #2E4A36 | #94A3B8 | Pie de página, disclaimers |
| ColorTagCategoria | #3D6B4F | #4B7A5C | Tags TURNO/URGENCIA/CERCANA |

### Estados de apertura (sin variante light)
| Token | Valor | Uso |
|-------|-------|-----|
| ColorAbiertaAhora | #22C55E | Farmacia abierta confirmada |
| ColorPosiblemente | #F59E0B | Posiblemente abierta |
| ColorNoConfirmado | #6B7280 | Horario no confirmado |
| ColorCerrada | #4B5563 | Farmacia cerrada |
| ColorUrgencia | #3B82F6 | Farmacia de urgencia |

### Acciones (sin variante light)
| Token | Valor | Uso |
|-------|-------|-----|
| ColorNavegacion | #3B82F6 | Botones de navegación |
| ColorLlamar | #22C55E | Botón llamar |
| ColorPeligro | #EF4444 | Acciones destructivas |

### Advertencia y Error (con variantes dark/light)
| Token | Dark | Light |
|-------|------|-------|
| ColorAdvertenciaFondo | #2A2000 | #FFFBEB |
| ColorAdvertenciaBorde | #CA8A04 | #F59E0B |
| ColorAdvertenciaTexto | #FCD34D | #92400E |
| ColorErrorFondo | #2A1F1F | #FEF2F2 |
| ColorErrorBorde | #6B2626 | #FECACA |
| ColorErrorTexto | #F87171 | #991B1B |
| ColorErrorBoton | #B91C1C | #DC2626 |

## 10. Infraestructura de deploy

- **Keystore release:** `farmapp-release.keystore` (raíz del proyecto)
- **APK firmado:** `FarmApp/bin/Release/net8.0-android/android-arm64/cl.farmapp.farmaciaabierta-Signed.apk`
- **Play Console:** No configurada aún
