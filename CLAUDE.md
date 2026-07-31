# CLAUDE.md — FarmApp

> Referencia rápida para sesiones de Claude Code. Fuente de verdad: el código del proyecto.

## Qué es

App .NET MAUI 10 (.NET 10) para encontrar farmacias de turno nocturno en Chile. Consume la API pública MIDAS/MINSAL, calcula distancias GPS con Haversine, y presenta resultados ordenados por proximidad. Monetización freemium: banner AdMob + compra única "FarmApp Pro" (`docs/MONETIZACION.md`).

- **App ID:** `cl.farmapp.farmaciaabierta`
- **Target principal:** Android (arm64), iOS incluido
- **Estado:** MVP funcional + monetización implementada (v1.1, versionCode 5). Pendiente: IDs reales de AdMob, producto `farmapp_pro` en Play Console y subida del AAB.
- **SDK:** anclado por `global.json` a .NET 10.0.1xx (workloads `android` + `maui` instalados vía VS).

## Build y validación

```bash
# Build Android debug (verificación rápida)
dotnet build FarmApp/FarmApp.csproj -f net10.0-android -c Debug -p:AndroidSdkDirectory=C:\Users\hecto\AppData\Local\Android\Sdk

# Build Android release firmado — AAB (requerido por Play Store)
# La contraseña se pasa por variable de entorno; NUNCA escribirla en archivos del repo (es público)
dotnet publish FarmApp/FarmApp.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=aab -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=../farmapp-release.keystore -p:AndroidSigningKeyAlias=farmapp-key -p:AndroidSigningKeyPass=$FARMAPP_STOREPASS -p:AndroidSigningStorePass=$FARMAPP_STOREPASS -p:AndroidSdkDirectory=C:\Users\hecto\AppData\Local\Android\Sdk
```

## Protección de código (Release)

- **R8 (AndroidLinkMode=SdkOnly):** ofusca nombres Java, elimina código muerto del SDK
- **Trimming .NET desactivado:** rompe `System.Text.Json` y `sqlite-net-pcl` (reflexión)
- Configurado en `FarmApp.csproj` bajo `<PropertyGroup Condition="'$(Configuration)' == 'Release'">`

## Advertencias de build conocidas (aceptadas)

- **CS0618 `Frame` obsoleto** (MAUI 10 recomienda `Border`): migración masiva no solicitada — no "arreglar" sin pedirlo
- **NU1608** (AndroidX fuera de rango): benigno, MAUI 10 trae AndroidX más nuevo que lo que piden los plugins
- **NU1903** (CVE-2025-6965 en `e_sqlite3` ≤2.1.11): sin parche upstream aún; riesgo bajo (solo SQL propio sobre caché local). Revisar si sale SQLitePCLRaw >2.1.11

## Reglas obligatorias

1. Lista de resultados usa `BindableLayout` en `VerticalStackLayout`, NO `CollectionView` (conflicto de gestos en Android)
2. No cambiar stack (.NET MAUI 10, CommunityToolkit, SQLite, Leaflet/WebView, Plugin.InAppBilling, Plugin.MauiMTAdmob)
3. No duplicar registro DI de `GeocodingService` (ya registrado vía AddHttpClient)
4. No proponer refactor masivo no solicitado
5. Respetar nombres existentes de clases, métodos, propiedades, keys de colores
6. No mezclar capas (Domain/Application/Infrastructure/Presentation)
7. Un cambio a la vez, con build de verificación
8. Leer archivo completo antes de modificarlo
9. Código y comentarios en español
10. Colores: definir ambas variantes dark (`ColorNombre`) y light (`ColorNombreLight`)
11. DI: Singleton para servicios stateless, Transient para ViewModels y Pages
12. Colores de estado fijos: verde `#22C55E` (abierta), amarillo `#F59E0B` (posiblemente), azul `#3B82F6` (urgencia), gris `#6B7280` (no confirmado)
13. Validar con build Android tras cada cambio
14. Referenciar `archivo.cs:línea` al citar código
15. El nombre de la app es **FarmApp**, NO "Farmacia Abierta"
16. Monetización: el dato MINSAL (qué farmacia está de turno, dónde, teléfono) NUNCA va detrás del paywall; solo banner, sin intersticiales
17. El producto IAP es `farmapp_pro` (`MonetizacionConstants.ProductoProId`) — debe coincidir con Play Console
18. Los IDs de AdMob del código son DE PRUEBA — reemplazar por reales solo al publicar (manifest + `MonetizacionConstants`)
19. Toda compra debe reconocerse con `FinalizePurchaseAsync` (sin acknowledge, Google reembolsa a los 3 días)
20. No escribir contraseñas/secretos en archivos del repo — es público en GitHub

## Arquitectura

```
Presentation  (XAML + ViewModels + Controls)
     ↓ invoca
Application   (BuscarFarmaciasUseCase)
     ↓ usa interfaces de
Domain        (Models + Interfaces + Services puros)
     ↑ implementado por
Infrastructure (API + Cache + Location)
```

## Archivos clave

| Archivo | Responsabilidad |
|---------|----------------|
| `MauiProgram.cs` | DI container, registro de todos los servicios |
| `Application/BuscarFarmaciasUseCase.cs` | Orquestación completa de búsqueda |
| `Domain/Models/Farmacia.cs` | Entidad principal, SQLite entity |
| `Infrastructure/Api/MinSalApiService.cs` | Cliente HTTP MIDAS/MINSAL |
| `Infrastructure/Api/GeocodingService.cs` | Nominatim OSM + throttle + GeoCache |
| `Infrastructure/Cache/DatabaseConnection.cs` | Conexión SQLite singleton compartida |
| `Presentation/ViewModels/ResultadosViewModel.cs` | Lista, filtro por radio (persistente si Pro), mapa |
| `Infrastructure/Compras/ComprasService.cs` | Google Play Billing → estado Pro (`IProService`) |
| `Presentation/Pages/ProPage.xaml` | Paywall: beneficios, comprar, restaurar, ajustes Pro |
| `Constants/MonetizacionConstants.cs` | IDs AdMob (⚠️ de prueba) y producto `farmapp_pro` |
| `Presentation/Controls/MiniMapView.xaml` | WebView Leaflet con JS bridge |
| `Resources/Raw/farmacia_map.html` | Leaflet embebido (offline, archivos locales) |
| `Constants/AppConstants.cs` | URLs, timeouts, radios, nombre DB |

## Dependencias externas

| Paquete | Versión | Uso |
|---------|---------|-----|
| Microsoft.Maui.Controls | 10.0.90 | Framework MAUI |
| CommunityToolkit.Maui | 15.0.0 | Componentes UI extras |
| CommunityToolkit.Mvvm | 8.3.2 | MVVM source generators |
| Microsoft.Extensions.Http | 10.0.10 | HttpClient tipado con DI |
| Microsoft.Extensions.Logging.Debug | 10.0.10 | Logging en debug |
| sqlite-net-pcl | 1.9.172 | ORM ligero para SQLite |
| SQLitePCLRaw.bundle_green | 2.1.11 | Provider nativo SQLite |
| Plugin.InAppBilling | 10.0.0 | Compras in-app (Billing Library 8.1) |
| Plugin.MauiMTAdmob | 2.4.0 | AdMob (GMA SDK 24) — ⚠️ namespace `Plugin.MauiMtAdmob` con "t" minúscula |

## APIs externas

- **MIDAS/MINSAL:** `GET https://midas.minsal.cl/farmacia_v2/WS/getLocalesTurnos.php` — sin auth, timeout 10s
- **Nominatim OSM:** `GET https://nominatim.openstreetmap.org/search` — rate limit 1 req/s, User-Agent obligatorio
- **Google Play Billing:** vía `Plugin.InAppBilling` (sin backend; titularidad cacheada en Preferences)
- **AdMob:** vía `Plugin.MauiMTAdmob` (App ID en AndroidManifest, unidad banner en `MonetizacionConstants`)
