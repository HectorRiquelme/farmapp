# NEXT_STEPS.md — FarmApp

> Pendientes reales verificados contra el código. Última actualización: 2026-03-23.

---

## Qué se hizo en esta sesión (2026-03-23)

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
| `PROJECT_CONTEXT.md` | Paleta de colores incompleta → reescrita con todos los tokens de `Colors.xaml` |
| `PROJECT_CONTEXT.md` | Faltaba `Properties/launchSettings.json` en estructura → agregado |
| `PROJECT_CONTEXT.md` | `AndroidManifest.xml` decía 3 permisos → corregido a 5 (`ACCESS_NETWORK_STATE`, `INTERNET`, `FINE_LOCATION`, `COARSE_LOCATION`, `CALL_PHONE`) |
| `.claude/memory/project_farmapp.md` | Actualizado: P1-P8 marcados como completados, pendientes son operativos |

### 4. Política de privacidad
| Archivo | Acción |
|---------|--------|
| `docs/privacy-policy.html` | **Creado** — HTML completo con 9 secciones, tema dark, contacto hectorariquelmec@gmail.com |
| Commit | `2deb2ff` — `docs: agregar política de privacidad para Google Play` |
| **Falta** | Activar GitHub Pages para que tenga URL pública (`https://hectorriquelme.github.io/farmapp/privacy-policy.html`) |

### 5. Renombramiento "Farmacia Abierta" → "FarmApp"
| Archivo | Qué se cambió |
|---------|---------------|
| `FarmApp/FarmApp.csproj` | `<ApplicationTitle>` → `FarmApp` |
| `FarmApp/AppShell.xaml` | `Title=` → `FarmApp` |
| `docs/privacy-policy.html` | 5 referencias actualizadas |
| `CLAUDE.md` | Título corregido |
| `AGENTS.md` | Título corregido |
| `PROJECT_CONTEXT.md` | Título y nombre corregidos |
| `NEXT_STEPS.md` | Título corregido |
| `.claude/memory/feedback_coding_rules.md` | Regla #15 agregada: nombre es FarmApp, no "Farmacia Abierta" |
| Commit | `ae9845a` — `fix: renombrar app de "Farmacia Abierta" a "FarmApp"` |

### 6. Limpieza
| Archivo | Acción | Razón |
|---------|--------|-------|
| `PROMPT_MAESTRO_CONTINUIDAD.md` | **Eliminado** | Contenía información de otro proyecto (portfolio React) mezclada con FarmApp; reemplazado por los 4 archivos nuevos |
| `FarmApp/*.png` (19 archivos) | **Eliminados** | Capturas de desarrollo basura (~5 MB) |
| `nul` | **Eliminado** | Artefacto de Windows |

### 7. Repositorio git
| Acción | Detalle |
|--------|---------|
| `git init` | Repositorio inicializado desde cero |
| `.gitignore` creado | Excluye `bin/`, `obj/`, `.vs/`, `*.keystore`, `FarmApp/*.png`, `nul` |
| Rama `master` → `main` | Renombrada con `git branch -m master main` |
| Remote configurado | `origin` → `https://github.com/HectorRiquelme/farmapp.git` |
| 5 commits en `main` | `8cf2ec2` → `ca2bd5a` → `0c0c8dd` → `2deb2ff` → `ae9845a` |

---

## Qué quedó pendiente

### 1. [ALTA] Activar GitHub Pages ← SIGUIENTE PASO
- **Qué:** Publicar `docs/privacy-policy.html` con URL pública
- **Archivo:** No requiere cambios de código — se configura en GitHub
- **Cómo:** Ir a `github.com/HectorRiquelme/farmapp` → Settings → Pages → Source: `main` branch, carpeta `/docs`
- **Resultado esperado:** `https://hectorriquelme.github.io/farmapp/privacy-policy.html` accesible públicamente
- **Bloqueante para:** Paso 4 (Play Console necesita URL de política de privacidad)

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
- **Qué:** Instalar APK en Samsung S23 Ultra y/o Xiaomi MIUI
- **Archivo:** No requiere cambios de código
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
- **Depende de:** Paso 1 (URL política) + Paso 2 (AAB)
- **Requisitos:**
  - AAB firmado
  - Política de privacidad con URL pública
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

### 5. [BAJA] Limpiar código muerto
- **Qué:** 3 constantes definidas sin uso
- **Archivo:** `FarmApp/Constants/AppConstants.cs:29-31`
- **Constantes:** `PrefRadioKm`, `PrefTemaApp`, `PrefUltimaComuna`
- **Acción:** Eliminar si no se planea usarlas, o dejar si se implementarán preferencias de usuario en v2

### 6. [BAJA] Eliminar dotnet_bot.png
- **Qué:** Asset default de la plantilla MAUI, no se usa en la app
- **Archivo:** `FarmApp/Resources/Images/dotnet_bot.png`
- **Acción:** Eliminar y quitar la referencia en `FarmApp.csproj:51`

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

### Sesión 2026-03-23 — Contexto + Git + Docs
- [x] Auditoría completa del proyecto (todos los archivos fuente leídos)
- [x] `CLAUDE.md` creado con reglas, build, arquitectura, dependencias
- [x] `AGENTS.md` reescrito (reemplazó contenido de otro proyecto)
- [x] `PROJECT_CONTEXT.md` creado + 5 correcciones post-auditoría
- [x] `NEXT_STEPS.md` creado con pendientes reales
- [x] `.claude/memory/project_farmapp.md` actualizado
- [x] `PROMPT_MAESTRO_CONTINUIDAD.md` eliminado
- [x] 19 capturas `.png` basura eliminadas + archivo `nul` eliminado
- [x] `.gitignore` creado (bin, obj, vs, keystore, screenshots)
- [x] Git inicializado, rama renombrada `master` → `main`
- [x] Remote configurado: `origin` → `github.com/HectorRiquelme/farmapp.git`
- [x] `docs/privacy-policy.html` creado (política de privacidad para Play Store)
- [x] Renombrado "Farmacia Abierta" → "FarmApp" en `.csproj`, `AppShell.xaml`, `docs/`, 4 archivos `.md`
- [x] Regla #15 agregada en `.claude/memory`: nombre es FarmApp, no "Farmacia Abierta"
- [x] 5 commits pusheados a `main` en GitHub

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

**Activar GitHub Pages** en el repo `farmapp`:
1. Ir a `https://github.com/HectorRiquelme/farmapp/settings/pages`
2. Source: Deploy from a branch → Branch: `main` → Folder: `/docs`
3. Guardar → esperar ~1 minuto
4. Verificar que `https://hectorriquelme.github.io/farmapp/privacy-policy.html` carga correctamente
5. Con esa URL lista, ya se puede completar la ficha en Play Console
