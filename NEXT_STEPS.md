# NEXT_STEPS.md — FarmApp / Farmacia Abierta

> Pendientes reales verificados contra el código. Última actualización: 2026-03-22.

---

## Qué se hizo en esta sesión (2026-03-22)

### 1. Auditoría completa del proyecto
- Se leyeron **todos** los archivos fuente (.cs, .xaml, .csproj, .html, .xml, .plist)
- Se verificó que las 6 tareas de código (P1–P3, P6–P8) ya estaban implementadas
- Build Android debug: **0 errores, 0 warnings**

### 2. Sistema de contexto persistente creado
| Archivo | Acción | Contenido |
|---------|--------|-----------|
| `CLAUDE.md` | **Creado** (estaba vacío) | Reglas obligatorias, build commands, arquitectura, archivos clave, dependencias, APIs |
| `AGENTS.md` | **Reescrito** (tenía contenido de otro proyecto — portfolio React) | Guía para agentes: capas, convenciones, decisiones fijas, flujo, errores comunes |
| `PROJECT_CONTEXT.md` | **Creado** (estaba vacío) | Contexto completo: identidad, estado, stack, estructura, modelo de dominio, flujo, paleta de colores, deploy |
| `NEXT_STEPS.md` | **Creado** (estaba vacío) | Pendientes reales verificados contra código |

### 3. Correcciones aplicadas tras auditoría
| Archivo | Qué se corrigió |
|---------|-----------------|
| `PROJECT_CONTEXT.md` | `Farmacia.Estado` documentado como `[Ignore]` → corregido: **se persiste en SQLite** (no tiene `[Ignore]`) |
| `PROJECT_CONTEXT.md` | Faltaban 3 constantes `PrefRadioKm`, `PrefTemaApp`, `PrefUltimaComuna` → agregadas con nota "sin uso actual" |
| `PROJECT_CONTEXT.md` | Paleta de colores incompleta → reescrita con todos los tokens de `Colors.xaml` (base, texto, estados, acciones, advertencia, error) |
| `.claude/memory/project_farmapp.md` | Actualizado: P1-P8 marcados como completados, pendientes son operativos |

### 4. Limpieza
| Archivo | Acción | Razón |
|---------|--------|-------|
| `PROMPT_MAESTRO_CONTINUIDAD.md` | **Eliminado** | Contenía información de otro proyecto (portfolio React) mezclada con FarmApp; estaba desactualizado; reemplazado por los 4 archivos nuevos |

### 5. Hallazgos durante la auditoría
- **Código muerto detectado:** `AppConstants.cs:29-31` define `PrefRadioKm`, `PrefTemaApp`, `PrefUltimaComuna` sin que ningún otro archivo las use
- **Sin repositorio git:** el proyecto no tiene `.git` inicializado

---

## Qué quedó pendiente

### 1. [ALTA] Política de privacidad ← SIGUIENTE PASO
- **Qué:** Crear documento de política de privacidad accesible vía URL pública
- **Archivo:** Crear `privacy-policy.html` o publicar en GitHub Pages / sitio externo
- **Por qué:** Google Play la exige obligatoriamente para apps que usan ubicación
- **Bloqueante para:** Publicar en Play Store
- **Contenido mínimo a cubrir:**
  - Datos recopilados: ubicación del dispositivo (solo mientras se usa, nunca almacenada en servidor)
  - Datos NO recopilados: información personal, cuentas, contactos, analytics
  - Procesamiento 100% local (no hay backend propio)
  - APIs de terceros: MINSAL (datos públicos de farmacias), Nominatim OSM (geocodificación, IP visible)
  - Caché local: SQLite con farmacias y coordenadas geocodificadas (auto-limpieza cada 2 días)
  - Sin cookies, sin tracking, sin publicidad
  - Contacto del desarrollador
- **Decisiones del usuario necesarias:**
  - Dónde publicar la política (GitHub Pages, hosting propio, otro)
  - Email de contacto para la política

### 2. [ALTA] Generar AAB (Android App Bundle)
- **Qué:** Play Store requiere AAB, no APK
- **Archivo:** No requiere cambio de código, solo comando de build
- **Comando:**
  ```bash
  dotnet publish FarmApp/FarmApp.csproj -f net8.0-android -c Release \
    -p:AndroidPackageFormat=aab \
    -p:AndroidKeyStore=true \
    -p:AndroidSigningKeyStore=../farmapp-release.keystore \
    -p:AndroidSigningKeyAlias=farmapp-key
  ```
- **Prerequisito:** Tener la contraseña del keystore disponible
- **Output esperado:** `FarmApp/bin/Release/net8.0-android/android-arm64/cl.farmapp.farmaciaabierta-Signed.aab`

### 3. [ALTA] Validación en dispositivo físico
- **Qué:** Instalar APK/AAB en Samsung S23 Ultra y/o Xiaomi MIUI
- **Archivo:** No requiere cambios
- **Comando:** `adb install -r <ruta-al-apk-firmado>`
- **Checklist de verificación:**
  - [ ] Permiso de ubicación se solicita correctamente
  - [ ] Mapa Leaflet carga y los pines son interactivos (tap abre popup)
  - [ ] Scroll de lista no entra en conflicto con WebView (BindableLayout)
  - [ ] Tema claro/oscuro se adapta al cambiar en ajustes del sistema
  - [ ] Botón "Llamar" abre el dialer con número correcto
  - [ ] Botón "Cómo llegar" abre Google Maps / app de mapas
  - [ ] Botón "Copiar dirección" copia al portapapeles
  - [ ] Sin conexión: muestra caché con advertencia y fecha
  - [ ] Slider de radio re-filtra la lista y recarga el mapa

### 4. [ALTA] Configuración Google Play Console
- **Qué:** Crear la ficha completa de la app
- **Archivo:** No requiere cambios de código
- **Requisitos:**
  - AAB firmado (depende del paso 2)
  - Política de privacidad con URL pública (depende del paso 1)
  - Data Safety form:
    - Ubicación: recopilada, no compartida, procesada localmente
    - Datos de uso de la app: no recopilados
    - No hay cuenta de usuario ni login
  - Capturas de pantalla: teléfono (mínimo 2) + tablet (recomendado)
  - Descripción corta (≤80 chars): "Encuentra la farmacia de turno más cercana en Chile"
  - Descripción larga (≤4000 chars): explicar funcionalidades, fuente MINSAL, offline
  - Icono de alta resolución: 512×512 PNG (sin alfa)
  - Gráfico de funciones: 1024×500 PNG o JPG
  - Categoría: Salud y bienestar → Medicina
  - Clasificación de contenido: completar cuestionario IARC
  - Países: Chile (o todos)
- **Decisiones del usuario necesarias:**
  - ¿Cuenta de Google Play Console existe o hay que crearla? (costo: $25 USD único)
  - ¿Distribución solo Chile o global?
  - ¿App gratuita? (una vez marcada como gratuita, no se puede cambiar a de pago)

### 5. [BAJA] Inicializar repositorio git
- **Qué:** El proyecto no tiene `.git`
- **Archivo:** Raíz del proyecto
- **Comando:** `git init && git add -A && git commit -m "feat: MVP farmacia abierta v1.0"`
- **Por qué:** Sin control de versiones no hay forma de rastrear cambios ni hacer rollback
- **Decisión del usuario:** ¿Crear repo en GitHub? ¿Público o privado?

### 6. [BAJA] Limpiar código muerto
- **Qué:** 3 constantes definidas sin uso
- **Archivo:** `FarmApp/Constants/AppConstants.cs:29-31`
- **Constantes:** `PrefRadioKm`, `PrefTemaApp`, `PrefUltimaComuna`
- **Acción:** Eliminar si no se planea usarlas, o dejar si se implementarán preferencias de usuario en v2

---

## Mejoras futuras (no bloqueantes para lanzamiento)

| Prioridad | Mejora | Archivo(s) involucrados |
|-----------|--------|------------------------|
| Media | Notificaciones push | Nuevo servicio + permisos Android/iOS |
| Media | Favoritos | `Domain/Models/`, `Infrastructure/Cache/`, nueva tabla SQLite |
| Media | Historial de búsquedas | `Infrastructure/Cache/`, nuevo ViewModel |
| Baja | Widget Android | `Platforms/Android/`, nuevo widget provider |
| Baja | Animaciones de transición | `Presentation/Pages/*.xaml` |
| Baja | Tests unitarios | Nuevo proyecto `FarmApp.Tests/` para AperturaService, GeoDistanciaService, ApiNormalizer |
| Baja | iOS TestFlight | Configurar en App Store Connect, requiere cuenta Apple Developer ($99/año) |

---

## Tareas completadas (referencia histórica)

### Sesión 2026-03-22 — Sistema de contexto
- [x] Auditoría completa del proyecto (todos los archivos fuente leídos)
- [x] `CLAUDE.md` creado con reglas, build, arquitectura, dependencias
- [x] `AGENTS.md` reescrito (reemplazó contenido de otro proyecto)
- [x] `PROJECT_CONTEXT.md` creado con contexto completo + 3 correcciones post-auditoría
- [x] `NEXT_STEPS.md` creado con pendientes reales
- [x] `.claude/memory/project_farmapp.md` actualizado
- [x] `PROMPT_MAESTRO_CONTINUIDAD.md` eliminado (reemplazado por los 4 archivos nuevos)

### Previo a esta sesión — Código
- [x] P1: Hardening SQLite — `DatabaseConnection` singleton en `Infrastructure/Cache/DatabaseConnection.cs`
- [x] P2: CancellationToken + Take(10) — `Application/BuscarFarmaciasUseCase.cs:173-185`
- [x] P3: Leaflet offline — `Resources/Raw/leaflet.min.js` + `leaflet.min.css`
- [x] P6: FarmaciaCardDestacada eliminada — verificado: no existe en ningún archivo fuente
- [x] P7: iOS permisos — `Platforms/iOS/Info.plist:32-33`
- [x] P8: Limpieza registros — `Application/BuscarFarmaciasUseCase.cs:48`
- [x] Build Android: 0 errores, 0 warnings

---

## Siguiente paso exacto

**Crear la política de privacidad.** Es el bloqueante #1 para Play Store y no depende de ningún otro paso. Necesito que definas:
1. ¿Dónde publicarla? (GitHub Pages, hosting propio, otro)
2. ¿Email de contacto para incluir en la política?
