# MONETIZACION.md — FarmApp

> Estrategia de monetización, decisiones y checklist de lanzamiento.
> Creado: 2026-07-31. La infraestructura de código ya está implementada y compila.

---

## 1. Modelo elegido: Freemium en UNA sola app

**Una única ficha en Play Store.** La app es gratis con un banner discreto; una compra única
("FarmApp Pro") quita los anuncios y desbloquea comodidades. No hay dos APKs ni dos fichas.

| | Versión gratis | FarmApp Pro (compra única) |
|---|---|---|
| Buscar farmacia de turno (MINSAL) | ✅ Completo | ✅ Completo |
| Mapa, distancias, llamar, navegar | ✅ | ✅ |
| Anuncios | Banner en resultados | ❌ Sin anuncios |
| Radio de búsqueda recordado | — | ✅ |
| Tema claro/oscuro manual | — | ✅ |
| Futuras (favoritos, widget, avisos) | — | ✅ (roadmap) |

### Por qué este modelo

1. **La app es de uso ocasional y de urgencia** (necesito una farmacia AHORA, de noche).
   Nadie paga por adelantado una app que usará 5 veces al año → una app "de pago" pura
   mataría la adquisición de usuarios.
2. **El dato core es público (MINSAL)** — cobrar por información esencial de salud sería
   éticamente cuestionable, generaría reseñas de 1 estrella y probablemente clones gratis.
   El core queda gratis SIEMPRE; se cobra por comodidad, no por el dato.
3. **Sin backend propio** → una suscripción es difícil de justificar (no hay costos
   recurrentes visibles ni features de servidor). La compra única es simple, honesta y
   no requiere validación de recibos en servidor.
4. **Una sola ficha** concentra instalaciones, reseñas y posicionamiento ASO. Dos fichas
   (free + paid) es el modelo antiguo: divide métricas y duplica mantenimiento.

### Alternativas evaluadas (y por qué se descartaron)

| Modelo | Veredicto | Razón |
|---|---|---|
| App de pago desde el día 1 | ❌ | Uso ocasional → nadie la compra sin probarla. Además Play no permite convertir una app gratuita en de pago después (la ficha actual ya es gratuita). |
| Dos apps (Free + Pro) | ❌ | Divide reseñas/instalaciones, doble QA y publicación. Obsoleto desde que existen IAP. |
| Suscripción mensual/anual | ⏸ Hoy no | Sin features recurrentes (avisos, sync) que la justifiquen. Reevaluar si se agrega notificación diaria de turnos o backend. |
| Solo anuncios (sin Pro) | ❌ | Techo de ingresos muy bajo con uso ocasional; el Pro captura a quienes odian los ads y financia el desarrollo. |
| Intersticiales / rewarded | ❌ | Interrumpir a alguien buscando farmacia de urgencia de noche = pésima UX y riesgo de política de Google en apps de salud. Solo banner. |
| Auspicio B2B (cadenas, labs, delivery) | ⏸ Futuro | Interesante con tracción (>10-20k usuarios activos). Ej: "entrega a domicilio con X" como acción en el detalle. No requiere cambiar el modelo actual. |
| Donaciones | ❌ | Ingreso marginal; el botón Pro cumple ese rol con mejor conversión. |

---

## 2. Qué quedó implementado en el código

| Pieza | Archivo | Nota |
|---|---|---|
| IDs y producto | `Constants/MonetizacionConstants.cs` | ⚠️ Contiene IDs DE PRUEBA de AdMob — reemplazar |
| Interfaz Pro | `Domain/Interfaces/IProService.cs` | Dominio puro, sin dependencia de plugins |
| Resultado compra | `Domain/Models/ResultadoCompraPro.cs` | Enum + record |
| Compras Google Play | `Infrastructure/Compras/ComprasService.cs` | Plugin.InAppBilling 10 (Billing Library 8.1) — connect → purchase → **acknowledge** → cache en Preferences |
| Paywall | `Presentation/Pages/ProPage.xaml(.cs)` + `ProViewModel` | Beneficios, precio localizado, comprar, restaurar, ajustes Pro |
| Banner AdMob | `ResultadosPage.xaml.cs` → `ActualizarZonaBanner()` | Solo se construye si NO es Pro; con enlace "Quitar anuncios" |
| App ID AdMob | `Platforms/Android/AndroidManifest.xml` | ⚠️ meta-data con App ID DE PRUEBA — reemplazar |
| Features Pro | `ResultadosViewModel` (radio persistente) + `ProViewModel`/`App` (tema) | Usan `PrefRadioKm` y `PrefTemaApp` |
| Registro DI + plugin | `MauiProgram.cs` | `.UseMauiMTAdmob()` + `IProService` singleton |

Detalles de comportamiento:

- La compra se **reconoce (acknowledge)** al completarse; sin eso Google reembolsa a los 3 días.
- Al arrancar, si el usuario es Pro se **revalida en segundo plano** (detecta reembolsos). Nunca bloquea el arranque.
- Si el usuario reinstala la app, recupera Pro con el botón **"Restaurar compras"** (o al intentar comprar de nuevo: Google responde "ya lo posees" y se reactiva solo).
- Sin backend: la fuente de verdad es Google Play + caché local. Para una app de este tamaño es el estándar aceptado.

---

## 3. Checklist de lanzamiento (en orden)

### A. Cuenta y app en AdMob (~30 min)
1. Crear cuenta en https://apps.admob.com con tu cuenta Google.
2. Apps → Agregar app → Android → "aún no está en Play" (se vincula después).
3. Crear **una unidad de anuncio Banner** (nombre sugerido: `banner_resultados`).
4. Copiar y reemplazar en el código:
   - **App ID** (`ca-app-pub-XXXX~YYYY`) → `Platforms/Android/AndroidManifest.xml` (meta-data `APPLICATION_ID`)
   - **ID de unidad banner** (`ca-app-pub-XXXX/ZZZZ`) → `MonetizacionConstants.AdMobBannerResultadosId`
5. Mientras desarrollas/pruebas deja los IDs de prueba (los actuales). **Nunca cliquees anuncios reales en tu propio dispositivo** (baneo de cuenta).

### B. Producto in-app en Play Console (~15 min)
1. Play Console → tu app → **Monetizar → Productos → Productos integrados** → Crear.
2. **ID del producto: `farmapp_pro`** (debe ser EXACTO — el código lo referencia así).
3. Nombre: "FarmApp Pro" · Descripción: "Sin anuncios + preferencias avanzadas, para siempre".
4. Precio: ver sección 4 (sugerido CLP $2.990). Estado: **Activo**.
5. Requisito previo: haber subido al menos un AAB con permiso BILLING a un track (el AAB nuevo ya lo trae vía la Billing Library).

### C. app-ads.txt (evita que AdMob limite la publicidad)
1. El "sitio del desarrollador" declarado en la ficha de Play debe servir `https://TU-DOMINIO/app-ads.txt` **en la raíz del dominio**.
2. Con GitHub Pages: crea el repo **`hectorriquelme.github.io`** (repo de usuario), activa Pages, y agrega el archivo `app-ads.txt` con la línea que AdMob te da (Apps → Ver todas → app-ads.txt), con esta forma:
   `google.com, pub-XXXXXXXXXXXXXXXX, DIRECT, f08c47fec0942fa0`
3. En la ficha de Play Store, declara como sitio web del desarrollador: `https://hectorriquelme.github.io`.
4. En AdMob: "Verificar app-ads.txt" (puede tardar días — no bloquea el lanzamiento).

### D. Ficha de Play: cambios obligatorios por los anuncios
- **Data Safety (Seguridad de los datos)** — actualizar:
  - La app AHORA muestra anuncios → declarar "Sí".
  - Datos recopilados por SDK de terceros (AdMob): *Identificadores de dispositivo (ID de publicidad)* — recopilado, compartido con fines publicitarios.
  - Ubicación: sigue igual (procesada localmente, no compartida).
- **Política de privacidad** (`docs/privacy-policy.html`): agregar sección de publicidad
  (uso de AdMob, ID de publicidad, enlace a políticas de Google) y de compras in-app.
- Mantener categoría **Mapas y navegación**.

### E. Consentimiento de anuncios (UMP)
- **Distribución solo Chile (recomendado para partir):** no se requiere el flujo de
  consentimiento GDPR. No hay que hacer nada.
- **Si distribuyes en Europa/UK:** configura el mensaje de consentimiento en AdMob
  (Privacidad y mensajería) — el plugin MauiMTAdmob 2.x soporta UMP.

### F. Probar compras sin pagar de verdad
1. Play Console → **Configuración → Prueba de licencias** → agrega tu Gmail (y el de testers).
2. Sube el AAB a **Prueba interna**, instala desde el enlace de la prueba.
3. Los testers de licencia ven tarjetas de prueba: "siempre se aprueba" / "siempre se rechaza".
4. Probar el ciclo completo: comprar → verificar que el banner desaparece → desinstalar →
   reinstalar → "Restaurar compras" → reembolsar desde Play Console → abrir la app (la
   revalidación de arranque debe quitar el Pro).

### G. Generar el AAB (versionCode 5, targetSdk 36, Billing 8)
```bash
$env:FARMAPP_STOREPASS="TU_PASSWORD"; dotnet publish FarmApp/FarmApp.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=aab -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=../farmapp-release.keystore -p:AndroidSigningKeyAlias=farmapp-key -p:AndroidSigningKeyPass=$env:FARMAPP_STOREPASS -p:AndroidSigningStorePass=$env:FARMAPP_STOREPASS -p:AndroidSdkDirectory=C:\Users\hecto\AppData\Local\Android\Sdk
```
> ⚠️ Nunca vuelvas a escribir la contraseña del keystore en un archivo del repo (ver NEXT_STEPS.md, sección seguridad).

---

## 4. Precio sugerido

| Concepto | Sugerencia |
|---|---|
| FarmApp Pro (única vez) | **CLP $2.990** (rango sano: $1.990–$3.990) |
| Comisión Google | 15% hasta USD 1M/año (inscribirse en el nivel del 15% en Play Console) |
| Neto aprox. por venta | ~CLP $2.540 |

Racional: es el precio de "un café" — decisión impulsiva, sin fricción. Más caro necesita
más features (guardar favoritos, widget, avisos). Puedes partir en $1.990 como "precio de
lanzamiento" y subirlo con el roadmap Pro.

---

## 5. Expectativa honesta de ingresos

La monetización **escala con los usuarios**; la infraestructura ya está lista, el trabajo
que sigue es adquisición (ASO + prensa/redes locales).

Supuestos conservadores: banner eCPM en Chile USD 0.3–1.0; sesiones cortas (1–3 páginas
de resultados por búsqueda); conversión a Pro típica de utilidades: 0.5–2% de usuarios activos.

| Escenario | Usuarios activos/mes | Ads (mes) | Pro (acumulado) |
|---|---|---|---|
| Inicio | 500 | ~USD 1–5 | 5–10 ventas ≈ CLP 15–30 mil |
| Tracción | 5.000 | ~USD 10–50 | 50–150 ventas ≈ CLP 150–450 mil |
| Referente local | 30.000 | ~USD 60–300 | 300–900 ventas ≈ CLP 0.9–2.7 M |

Palancas de crecimiento (orden de impacto):
1. **ASO**: título/descripción con "farmacia de turno", capturas nuevas, reseñas.
2. **Estacionalidad**: campañas de invierno (resfríos) y fines de semana largos.
3. **Prensa/RRSS locales**: apps de utilidad pública chilena tienen buena recepción.
4. **Roadmap Pro** (sección 6) para subir la conversión.

---

## 6. Roadmap Pro v2 (sube el valor de la compra)

| Feature | Esfuerzo | Impacto en conversión |
|---|---|---|
| Farmacias favoritas | Medio (tabla SQLite + UI) | Alto |
| Widget Android "turno hoy" | Medio | Alto |
| Notificación diaria "farmacia de turno en tu comuna" | Alto (WorkManager + permiso notificaciones) | Muy alto |
| Historial de búsquedas | Bajo | Medio |
| Modo offline extendido (>2 días de caché) | Bajo | Medio |

Regla: cada feature nueva de conveniencia entra al Pro; el dato MINSAL sigue gratis.

---

## 7. Reglas de oro (no romper)

1. **Nunca** poner el dato esencial (qué farmacia está de turno, dónde, teléfono) detrás del paywall.
2. **Solo banner** — nada de intersticiales/rewarded en flujo de urgencia.
3. Reemplazar los IDs de prueba de AdMob **antes** de publicar (manifest + constantes).
4. El producto IAP se llama **`farmapp_pro`** — si cambias el ID en Play Console, cambia `MonetizacionConstants.ProductoProId`.
5. No publicar actualizaciones con Billing Library <8 (rechazo automático desde 31-08-2026; la actual es 8.1 ✓).
6. Probar SIEMPRE compra + restauración + reembolso en prueba interna antes de producción.

---

## 8. Referencias

- Requisito Billing 8 (31-08-2026): https://developer.android.com/google/play/billing/deprecation-faq
- Migración Billing 8: https://developer.android.com/google/play/billing/migrate-gpblv8
- Plugin compras: https://github.com/jamesmontemagno/InAppBillingPlugin
- Plugin AdMob: https://github.com/marcojak/MauiMTAdmob (guía: https://hightouchinnovation.com/MMTAdmobGuide)
- app-ads.txt: https://support.google.com/admob/answer/9363762
- Nivel de comisión 15%: Play Console → Configuración → Programa de tarifas de servicio
