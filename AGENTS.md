# AGENTS.md — FarmApp

> Guía para agentes de IA que trabajen en este proyecto.

## Contexto del proyecto

FarmApp es una app .NET MAUI 8 que encuentra farmacias de turno nocturno en Chile usando datos oficiales del MINSAL. El proyecto sigue Clean Architecture con MVVM.

## Antes de modificar código

1. **Lee `CLAUDE.md`** para reglas obligatorias y arquitectura
2. **Lee el archivo completo** que vas a modificar (no asumas contenido)
3. **Verifica la capa** a la que pertenece el archivo — no mezclar capas
4. **Valida con build** después de cada cambio: `dotnet build FarmApp/FarmApp.csproj -f net8.0-android`

## Estructura de capas

```
FarmApp/
├── Domain/           ← Modelos, interfaces, servicios puros (sin dependencias externas)
├── Application/      ← Casos de uso (orquestación)
├── Infrastructure/   ← Implementaciones concretas (API, SQLite, GPS)
├── Presentation/     ← XAML, ViewModels, Controls, Converters
├── Constants/        ← Configuración centralizada
├── Resources/        ← Assets (iconos, fuentes, HTML, estilos XAML)
└── Platforms/        ← Código específico de plataforma
```

## Convenciones de código

- **Idioma:** Código y comentarios en español
- **MVVM:** ViewModels heredan de `BaseViewModel : ObservableObject`
- **Atributos:** Usar `[ObservableProperty]` y `[RelayCommand]` del CommunityToolkit
- **DI:** Singleton para servicios stateless, Transient para ViewModels y Pages
- **Colores XAML:** Siempre definir variante dark (`ColorNombre`) y light (`ColorNombreLight`)
- **Navegación:** Shell con `QueryProperty` para pasar datos entre páginas
- **Lista de resultados:** `BindableLayout` en `VerticalStackLayout` (NO `CollectionView`)

## Decisiones fijas (no reabrir)

| Decisión | Razón |
|----------|-------|
| BindableLayout, no CollectionView | Conflicto de gestos con WebView en Android |
| Leaflet en WebView | Único mapa viable en MAUI sin licencias comerciales |
| UserAppTheme = Unspecified | Respetar tema del sistema (bug anterior forzaba dark) |
| SQLite con DatabaseConnection singleton | Evita contención por conexiones concurrentes |
| Geocodificación con Take(10) + timeout 30s | Respetar rate limit Nominatim y batería |
| R8 + Trimming en Release | Ofuscación + reducción de tamaño AAB/APK |
| Nombre "FarmApp" (no "Farmacia Abierta") | Corregido por el usuario; aplicar en todo nuevo código/doc |
| Categoría Play Store: Mapas y navegación | Evita requisitos médicos extras de categoría Medicina |

## Flujo de búsqueda (referencia rápida)

1. `HomeViewModel` → solicita permiso GPS (popup nativo) → obtiene ubicación → llama `BuscarFarmaciasUseCase.EjecutarAsync()`
2. UseCase → verifica conectividad → API MIDAS o caché SQLite
3. Normaliza DTOs → calcula estado apertura → calcula distancias Haversine
4. Filtra por radio progresivo (5→15→50→200 km) → persiste en SQLite
5. Geocodifica faltantes en background (máx 10, timeout 30s)
6. Retorna `BusquedaResultado` (incluye `UbicacionUsuario`) → navega a `ResultadosPage`
7. ResultadosPage → carga mapa Leaflet vía JS bridge + pin azul de ubicación del usuario

## Errores comunes a evitar

- No registrar `GeocodingService` dos veces en DI (ya registrado vía `AddHttpClient`)
- No crear nuevas `SQLiteAsyncConnection` — usar `DatabaseConnection.Db`
- No usar `CollectionView` para la lista de farmacias
- No hardcodear colores fuera de `Colors.xaml`
- No modificar `farmacia_map.html` sin entender el JS bridge (`loadFarmacias`, `centrarEn`)
