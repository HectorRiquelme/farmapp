# NEXT_STEPS.md — FarmApp

> Pendientes reales verificados contra el código. Última actualización: 2026-03-26.

---

## Qué se hizo en sesión 2026-03-26

### 1. Correcciones pre-Play Store aplicadas (las 3 de la auditoría anterior)
| # | Archivo | Qué se corrigió | Verificado |
|---|---------|-----------------|-----------|
| 1 | `HomeViewModel.cs:33` | `Titulo = "Farmacia Abierta"` → `"FarmApp"` | ✅ Confirmado en auditoría |
| 2 | `AppConstants.cs:13` | Email falso `farmapp@ejemplo.cl` → `hectorariquelmec@gmail.com` | ✅ Confirmado en auditoría |
| 3 | `Resources/Images/dotnet_bot.png` | Eliminado asset default MAUI | ✅ Solo queda `logo_farmapp.svg` |
| — | Commit | `7a39deb` — `fix: correcciones pre-Play Store detectadas en auditoría` | ✅ Pusheado |

### 2. Keystore creado y AAB generado
| Paso | Detalle |
|------|---------|
| Keystore | `farmapp-release.keystore` creado con `keytool` (RSA 2048, validez 10000 días, CN=Hector Riquelme) |
| AAB firmado | `cl.farmapp.farmaciaabierta-Signed.aab` — **23 MB** |
| Ubicación | `FarmApp/bin/Release/net8.0-android/cl.farmapp.farmaciaabierta-Signed.aab` |

### 3. Auditoría QA final — APROBADA
| Requisito | Estado |
|-----------|--------|
| Nombre = "FarmApp" en todo el código | ✅ |
| Permisos Android (5) + iOS declarados | ✅ |
| Política de privacidad con nombre correcto | ✅ |
| R8 + Trimming en Release | ✅ |
| Icono y splash personalizados (no default) | ✅ |
| Sin assets basura | ✅ |
| User-Agent con email real | ✅ |
| Sin auth / analytics / ads / compras | ✅ |
| AAB firmado y generado | ✅ |
| 0 referencias "Farmacia Abierta" en código fuente | ✅ |

---

## Qué se hizo en sesión 2026-03-24

### 1. R8 + Trimming activado en Release
- **Archivo:** `FarmApp/FarmApp.csproj:43-49`
- **Qué:** `AndroidLinkMode=SdkOnly`, `PublishTrimmed=true`, `TrimMode=link`
- **Commit:** `cca698b`
- **Build verificado:** 0 errores, 0 warnings

### 2. Auditoría contra políticas Google Play
Se verificó la app contra requisitos de publicación. Resultado:

| Requisito | Estado |
|-----------|--------|
| Permisos declarados + runtime request | ✅ OK |
| Sin datos personales / login / analytics | ✅ OK |
| Política de privacidad | ✅ Creada |
| Sin publicidad / compras in-app | ✅ OK |
| Icono y splash personalizados | ✅ OK |
| targetSdk ≥ 34 | ✅ OK (workload android 34.0.154) |
| R8 + Trimming | ✅ Activado |

### 3. Problemas detectados (pendientes de corrección)
| Severidad | Archivo | Problema |
|-----------|---------|----------|
| 🔴 Obligatorio | `HomeViewModel.cs:33` | `Titulo = "Farmacia Abierta"` — debe ser `"FarmApp"` |
| 🟡 Recomendado | `AppConstants.cs:13` | User-Agent con email falso `farmapp@ejemplo.cl` → debe ser `hectorariquelmec@gmail.com` |
| 🟡 Recomendado | `FarmApp.csproj:60` + `Resources/Images/dotnet_bot.png` | Asset default MAUI sin uso, se empaqueta en el AAB |

### 4. Contexto persistente actualizado
- `CLAUDE.md` — agregado: build AAB, R8+Trimming, regla #15 nombre
- `AGENTS.md` — agregado: 3 decisiones fijas nuevas
- `PROJECT_CONTEXT.md` — estado actual actualizado a 2026-03-24, deploy ampliado
- `NEXT_STEPS.md` — esta actualización

---

## Qué se hizo en sesión 2026-03-23

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

### 1. [ALTA] Validación en dispositivo físico ← SIGUIENTE PASO
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

### 2. [ALTA] Configuración Google Play Console
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
  - Categoría: **Mapas y navegación** (evita requisitos médicos extras de categoría Medicina)
  - Clasificación de contenido: completar cuestionario IARC
  - Países: Chile (o todos)
- **Decisiones del usuario necesarias:**
  - ¿Cuenta de Google Play Console existe o hay que crearla? (costo: $25 USD único)
  - ¿Distribución solo Chile o global?
  - ¿App gratuita? (una vez marcada como gratuita, no se puede cambiar a de pago)

### 3. [BAJA] Limpiar código muerto
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

### Sesión 2026-03-26 — Correcciones + AAB + Auditoría final
- [x] 3 correcciones pre-Play Store aplicadas (HomeViewModel, AppConstants, dotnet_bot.png)
- [x] Keystore creado (RSA 2048, 10000 días)
- [x] AAB firmado generado (23 MB)
- [x] GitHub Pages activado — política de privacidad en URL pública
- [x] Auditoría QA final: todos los checks pasan
- [x] `NEXT_STEPS.md` actualizado con estado real
- [x] Commit `7a39deb` pusheado

### Sesión 2026-03-24 — R8 + Auditoría Google Play
- [x] R8 + Trimming activado en Release (`cca698b`)
- [x] Auditoría contra políticas Google Play (detectó 3 problemas)
- [x] 4 archivos de contexto actualizados

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

**Configurar Google Play Console:**
1. Crear cuenta de desarrollador en [Play Console](https://play.google.com/console) ($25 USD único) — si no existe
2. Crear nueva aplicación → nombre "FarmApp" → gratuita
3. Subir AAB: `FarmApp/bin/Release/net8.0-android/cl.farmapp.farmaciaabierta-Signed.aab`
4. Configurar ficha: descripción, capturas, icono 512x512, gráfico 1024x500
5. Data Safety: ubicación recopilada (local, no compartida), sin cuenta de usuario
6. Política de privacidad: `https://hectorriquelme.github.io/farmapp/privacy-policy.html`
7. Clasificación IARC: completar cuestionario
8. Categoría: Mapas y navegación
9. Publicar en revisión
