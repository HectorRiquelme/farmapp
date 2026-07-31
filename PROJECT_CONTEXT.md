# PROJECT_CONTEXT.md — FarmApp

> Contexto completo del proyecto. Última actualización: 2026-07-31.

## 1. Identidad

- **Nombre:** FarmApp
- **App ID:** `cl.farmapp.farmaciaabierta`
- **Versión:** 1.1 (ApplicationVersion=5)
- **Tipo:** App móvil multiplataforma (.NET MAUI 10 / .NET 10)
- **Target prioritario:** Android (API 24+, arm64, targetSdk 36)
- **Objetivo:** Encontrar la farmacia de turno nocturno más cercana en Chile usando datos oficiales MINSAL
- **Modelo de negocio:** Freemium — gratis con banner AdMob + compra única "FarmApp Pro" (`farmapp_pro`). Ver `docs/MONETIZACION.md`

## 2. Estado actual (verificado 2026-03-24)

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
- [x] Política de privacidad creada (`docs/privacy-policy.html`)
- [x] Renombramiento "Farmacia Abierta" → "FarmApp" en csproj, AppShell, docs, contexto
- [x] R8 + Trimming activado en Release (ofuscación + optimización)
- [x] Git inicializado, remote configurado en GitHub
- [x] Mapa Leaflet reactivo al tema del sistema (claro/oscuro)
- [x] Pin azul "Tu ubicación" en el mapa
- [x] Solicitud automática de permiso GPS (popup nativo Android)
- [x] Respuesta táctil del mapa mejorada (propagación a toda la jerarquía)
- [x] GitHub Pages activado — política de privacidad online
- [x] AAB versionCode 4 con targetSdk 35 generado (36 MB, con R8) — subido a prueba interna
- [x] Verificado en dispositivo físico Samsung S23 Ultra
- [x] **Migración a .NET 10 / MAUI 10** (2026-07-31): TFMs net10.0-*, targetSdk 36, minSdk 24, `global.json` ancla SDK 10.0.1xx, `CreateWindow` reemplaza `MainPage`, sin `Controls.Compatibility`
- [x] **Monetización freemium implementada** (2026-07-31): `IProService`/`ComprasService` (Plugin.InAppBilling 10, Billing 8.1, acknowledge + restauración + revalidación), banner AdMob en resultados (Plugin.MauiMTAdmob 2.4, solo no-Pro), `ProPage` paywall, radio persistente y tema manual para Pro
- [x] Build Debug y Release net10.0-android: 0 errores (warnings conocidos documentados en CLAUDE.md)

### Pendiente — operativo (acciones del desarrollador)
- [ ] Crear cuenta AdMob + unidad banner → reemplazar IDs de prueba (manifest + `MonetizacionConstants`)
- [ ] Crear producto `farmapp_pro` en Play Console (CLP $2.990 sugerido) y activarlo
- [ ] Actualizar Data Safety (ahora hay anuncios/advertising ID) y política de privacidad
- [ ] app-ads.txt en `hectorriquelme.github.io` (raíz del dominio)
- [ ] Probar compra/restauración/reembolso en prueba interna (license testers)
- [ ] Subir AAB versionCode 5 (net10, targetSdk 36, Billing 8) y completar ficha (capturas, IARC)

## 3. Stack tecnológico

| Componente | Tecnología | Versión |
|------------|-----------|---------|
| Framework | .NET MAUI (Microsoft.Maui.Controls) | 10.0.90 (.NET 10) |
| MVVM | CommunityToolkit.Mvvm | 8.3.2 |
| UI toolkit | CommunityToolkit.Maui | 15.0.0 |
| HTTP | Microsoft.Extensions.Http | 10.0.10 |
| Base de datos | sqlite-net-pcl | 1.9.172 |
| SQLite nativo | SQLitePCLRaw.bundle_green | 2.1.11 |
| Compras in-app | Plugin.InAppBilling | 10.0.0 (Billing Library 8.1) |
| Anuncios | Plugin.MauiMTAdmob | 2.4.0 (GMA SDK 24) — namespace `Plugin.MauiMtAdmob` ("t" minúscula) |
| Mapa | Leaflet.js | 1.9.4 (bundled) |
| Tiles | CartoDB Dark Matter | CDN online |
| Geocodificación | Nominatim OSM | API pública |
| API datos | MIDAS/MINSAL | API pública |

**Sin backend propio.** La app consume directamente las APIs públicas; la titularidad de
FarmApp Pro se consulta a Google Play y se cachea en Preferences (`pref_es_pro`).

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
│   ├── AppConstants.cs                   ← URLs, timeouts, radios, nombre DB
│   └── MonetizacionConstants.cs          ← IDs AdMob (⚠️ de prueba) + producto farmapp_pro
├── Domain/
│   ├── Models/
│   │   ├── Farmacia.cs                   ← Entidad principal (SQLite)
│   │   ├── BusquedaResultado.cs          ← Resultado de búsqueda
│   │   ├── UbicacionUsuario.cs           ← Record (Lat, Lon)
│   │   ├── ResultadoCompraPro.cs         ← EstadoCompraPro + record de resultado
│   │   └── Enums.cs                      ← TipoFarmacia, EstadoApertura, FuenteBusqueda
│   ├── Interfaces/
│   │   ├── IFarmaciaProvider.cs
│   │   ├── IFarmaciaRepository.cs
│   │   ├── IGeoCacheRepository.cs
│   │   ├── ILocationService.cs
│   │   └── IProService.cs                ← Estado y operaciones de FarmApp Pro
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
│   ├── Compras/
│   │   └── ComprasService.cs             ← Google Play Billing (IProService): compra, acknowledge, restauración, revalidación
│   └── Location/
│       └── MauiLocationService.cs        ← GPS vía MAUI Geolocation
├── Presentation/
│   ├── Pages/
│   │   ├── HomePage.xaml / .cs           ← Pantalla inicio con logo, botón buscar y acceso a Pro
│   │   ├── ResultadosPage.xaml / .cs     ← Lista + slider radio + miniMapa + banner AdMob (no-Pro)
│   │   ├── DetallePage.xaml / .cs        ← Detalle farmacia + llamar/navegar
│   │   └── ProPage.xaml / .cs            ← Paywall: beneficios, comprar, restaurar, ajustes Pro
│   ├── ViewModels/
│   │   ├── BaseViewModel.cs              ← EstaCargando, Titulo, NoEstaCargando
│   │   ├── HomeViewModel.cs              ← Búsqueda + permisos GPS + IrAPro
│   │   ├── ResultadosViewModel.cs        ← Filtrado + control mapa + radio persistente (Pro)
│   │   ├── DetalleFarmaciaViewModel.cs   ← Detalle + acciones
│   │   └── ProViewModel.cs               ← Compra/restauración + precio localizado + tema
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
| NominatimUserAgent | `FarmApp/1.0 (hectorariquelmec@gmail.com)` |
| CacheDiasMaximos | 2 |
| RadioInicialKm | 5.0 |
| RadioAmpliadoKm | 15.0 |
| RadioExtendidoKm | 50.0 |
| RadioMaximoKm | 200.0 |
| MaxResultadosLista | 20 |
| NombreBaseDatos | `farmapp.db` |
| PrefRadioKm | `pref_radio_km` — **en uso**: radio persistente para usuarios Pro |
| PrefTemaApp | `pref_tema_app` — **en uso**: tema manual para usuarios Pro |
| PrefUltimaComuna | `pref_ultima_comuna` (definida, sin uso actual) |

### MonetizacionConstants.cs

| Constante | Valor |
|-----------|-------|
| ProductoProId | `farmapp_pro` (no consumible; debe existir en Play Console con ese ID) |
| PrefEsPro | `pref_es_pro` |
| AdMobBannerResultadosId | ⚠️ ID DE PRUEBA de Google — reemplazar antes de publicar |

El App ID de AdMob (⚠️ también de prueba) vive en `Platforms/Android/AndroidManifest.xml`.

## 8. Flujo de búsqueda completo

1. Usuario toca "Buscar ahora" → `HomeViewModel.BuscarFarmaciasAsync()`
2. `MauiLocationService` solicita permiso GPS (popup nativo Android) + obtiene ubicación (timeout 15s). Si el permiso fue recién otorgado, reintenta automáticamente.
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
6. `MiniMapView` invoca JS: `loadFarmacias(json)` → pines en Leaflet + `setUserLocation(lat, lon)` → pin azul usuario
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

- **Keystore release:** `farmapp-release.keystore` (raíz del proyecto, excluido de git). ⚠️ La contraseña estuvo expuesta en este repo público hasta 2026-07-31 — ver NEXT_STEPS.md (sección seguridad); nunca escribirla en archivos del repo
- **Próximo AAB:** versionCode=5, versión 1.1, targetSdk=36, Billing Library 8.1 (pendiente de generar tras reemplazar IDs de AdMob)
- **Protección Release:** Solo R8 (`AndroidLinkMode=SdkOnly`). Trimming .NET **desactivado** (rompe JSON/SQLite reflection)
- **.NET SDK:** 10.0.1xx anclado por `global.json` (rollForward latestPatch) — workloads `android 36.1.43` + `maui 10.0.20` instalados vía VS. El SDK 10.0.2xx NO tiene workloads: no quitar el global.json
- **targetSdk:** 36 por defecto del workload (se eliminó el override `<uses-sdk>` del manifest y `<AndroidTargetSdkVersion>` del csproj); minSdk 24 vía `SupportedOSPlatformVersion`
- **Android SDK para build:** `C:\Users\hecto\AppData\Local\Android\Sdk` (pasar `-p:AndroidSdkDirectory` en build/publish)
- **GitHub:** `https://github.com/HectorRiquelme/app-farmapp.git` — rama `main` — repo **PÚBLICO**
- **Política de privacidad:** `docs/privacy-policy.html` — ⚠️ pendiente: agregar sección de anuncios (AdMob/advertising ID) y compras; verificar que la URL pública de GitHub Pages siga activa tras el cambio de nombre del repo
- **Play Console:** ficha creada; siguiente subida = AAB vc5 con monetización
- **Contacto desarrollador:** hectorariquelmec@gmail.com
