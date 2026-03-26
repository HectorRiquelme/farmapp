# CLAUDE.md — FarmApp

> Referencia rápida para sesiones de Claude Code. Fuente de verdad: el código del proyecto.

## Qué es

App .NET MAUI 8 para encontrar farmacias de turno nocturno en Chile. Consume la API pública MIDAS/MINSAL, calcula distancias GPS con Haversine, y presenta resultados ordenados por proximidad.

- **App ID:** `cl.farmapp.farmaciaabierta`
- **Target principal:** Android (arm64), iOS incluido
- **Estado:** MVP funcional. Build release firmado con R8+Trimming. Pendiente correcciones pre-Play Store y publicación.

## Build y validación

```bash
# Build Android debug (verificación rápida)
dotnet build FarmApp/FarmApp.csproj -f net8.0-android -c Debug

# Build Android release firmado (arm64) — APK
dotnet publish FarmApp/FarmApp.csproj -f net8.0-android -c Release -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=../farmapp-release.keystore -p:AndroidSigningKeyAlias=farmapp-key

# Build Android release firmado — AAB (requerido por Play Store)
dotnet publish FarmApp/FarmApp.csproj -f net8.0-android -c Release -p:AndroidPackageFormat=aab -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=../farmapp-release.keystore -p:AndroidSigningKeyAlias=farmapp-key
```

## Protección de código (Release)

- **R8 (AndroidLinkMode=SdkOnly):** ofusca nombres Java, elimina código muerto del SDK
- **Trimming (PublishTrimmed + TrimMode=link):** elimina código .NET no utilizado
- Configurado en `FarmApp.csproj` bajo `<PropertyGroup Condition="'$(Configuration)' == 'Release'">`

## Reglas obligatorias

1. Lista de resultados usa `BindableLayout` en `VerticalStackLayout`, NO `CollectionView` (conflicto de gestos en Android)
2. No cambiar stack (.NET MAUI 8, CommunityToolkit, SQLite, Leaflet/WebView)
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
| `Presentation/ViewModels/ResultadosViewModel.cs` | Lista, filtro por radio, mapa |
| `Presentation/Controls/MiniMapView.xaml` | WebView Leaflet con JS bridge |
| `Resources/Raw/farmacia_map.html` | Leaflet embebido (offline, archivos locales) |
| `Constants/AppConstants.cs` | URLs, timeouts, radios, nombre DB |

## Dependencias externas

| Paquete | Versión | Uso |
|---------|---------|-----|
| CommunityToolkit.Maui | 9.1.1 | Componentes UI extras |
| CommunityToolkit.Mvvm | 8.3.2 | MVVM source generators |
| Microsoft.Extensions.Http | 8.0.0 | HttpClient tipado con DI |
| sqlite-net-pcl | 1.9.172 | ORM ligero para SQLite |
| SQLitePCLRaw.bundle_green | 2.1.10 | Provider nativo SQLite |

## APIs externas

- **MIDAS/MINSAL:** `GET https://midas.minsal.cl/farmacia_v2/WS/getLocalesTurnos.php` — sin auth, timeout 10s
- **Nominatim OSM:** `GET https://nominatim.openstreetmap.org/search` — rate limit 1 req/s, User-Agent obligatorio
