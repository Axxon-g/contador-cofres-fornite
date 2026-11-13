# Desktop — Operate

Native Windows desktop, C# WPF (`net10.0-windows`). Source authority: `src/Contador/App.xaml`, `MainWindow.xaml`, and navigation/status assignments in `MainWindow.xaml.cs`. Root `DESIGN.md` records the extracted system; `PRODUCT.md` retains product truth.

The navy workspace, pirate-red navigation rail, ivory text and treasure-gold count/active navigation establish the night-session atmosphere. The user-supplied pirate token is embedded as a WPF Resource, displayed at 112 × 112 with Uniform stretch and accessible name “Moneda de tesoro pirata”; it needs no network image request. Segoe UI and Windows controls keep the application familiar. Ready state uses pale green; every status also has text. This is a code-led surface with no comp.

## Composition

Main window 1120 × 780; minimum 860 × 640, in WPF device-independent units. Fixed 200-unit navigation rail; flexible work area with 32 horizontal / 28 vertical inset. Persistent page title and footer frame five workspaces: Resumen, Historial, Rutas sugeridas, Informe, Ajustes. The shared reader status panel is hidden on routes and report; the report carries its own account, period and update text. Summary, routes, report and settings scroll vertically. History keeps a sortable read-only table and search/export actions.

The total is the strongest figure (76, SemiBold, gold); session count is secondary (42, SemiBold). Page titles use 28; section titles 20. Status and help panels use 10-unit corners; buttons use 8. Buttons have 18 × 11 padding, gold hover border and a 2-unit gold keyboard-focus border; disabled buttons use 0.45 opacity. Tables use 48-unit rows, alternating ground tones and visible selected-row fill. Depth is tonal rather than shadow-based. No ornamental motion is defined.

## Implementation boundary

### Rutas sugeridas (1.2.7)

La sección sustituye el análisis espacial de repeticiones por una selección editorial de recorridos y zonas de búsqueda de la comunidad. Un selector nativo cambia la propuesta; fecha de revisión, tipo de evidencia, resumen, pasos, cautelas y enlaces permiten valorar cada fuente. Los testimonios y fuentes secundarias se identifican como tales; no se clasifican zonas por premios ni se prometen probabilidades. La selección está guardada en la app y no se actualiza automáticamente desde redes. Los enlaces solo se abren a petición del usuario.

El mapa aportado por el usuario de 375 × 375 píxeles permanece integrado localmente y conserva su proporción cuadrada. GuideMap dibuja números de orden, círculos de sectores aproximados y una línea discontinua dorada con apoyo oscuro. Son diagramas editoriales: no usan posiciones del jugador, proyección de repeticiones, detección de cofres ni resultados observados. El texto declara que las líneas no son caminos ni posiciones exactas y que lugares, botín y competencia pueden cambiar. El historial y sus recuentos se conservan.

### Informe nativo (1.2.7)

El informe se consulta dentro de la aplicación y mantiene un botón explícito «Abrir versión HTML». Cuenta, período y actualización preceden las cifras. Comparte las reglas del HTML: cofres propios abiertos o destruidos, sesión desde la apertura de la app, media, récord, tesoros propios, últimos 30 días, desglose por tipo, interacciones excluidas del total, orígenes con fiabilidad, detalle de hallazgos y últimas 100 partidas. Se recalcula tras el análisis o al cambiar cuenta y fechas. La cobertura explica los totales importados sin desglose; el estado vacío indica que no hay partidas y remite a cuenta o fechas de Ajustes.

El total del informe usa 42 unidades y dorado; estadísticas secundarias usan 28 y se redistribuyen en un WrapPanel al estrechar la ventana. El gráfico diario incluye nombres accesibles y descripciones; un Expander ofrece los mismos datos en tabla. Las tablas conservan encabezados sin corte de palabra y desplazamiento horizontal cuando las columnas no caben. La casilla de equipo añade filas identificadas como estimaciones sin sumar sus tesoros a las cifras ni a la frecuencia propias. Estos contenidos mantienen Segoe UI, controles nativos, capas tonales y navegación del sistema existente.

Use native controls and OS folder/export dialogs. Preserve keyboard access, named inputs and state text. The inherited overlay is independently configurable. The replay reader remains the source of supported-format counting; do not show fabricated match data or imply live-game counting. Presence separately probes headers of recently written replay files (three-minute window), without reading the game process. The gold banner reads “EN PARTIDA · Cofres al finalizar” or “Esperando partida”; completed replay processing supplies the figures.

Session means completed matches since opening the app, excluding baseline history and historical imports. Dashboard, native overlay and report session filter share this rule, scoped to the selected account. Dashboard copy reads “Cofres de esta sesión” and “N partidas desde que abriste la app”. Overlay labels distinguish total, “Esta sesión”, “Última partida”, “Tesoros” and presence (“En partida” / “Esperando”); presence does not imply a live chest count.

`.impeccable/design.json` records extracted color metadata in schema version 2. Root DESIGN.md owns extracted token primitives in frontmatter; sidecar color metadata references the same names. No tonal ramps, browser component previews, web hook or detector were fabricated: this is WPF, with no web component library. Empty motion/shadow/breakpoint arrays mean none were extracted; window constraints are documented here rather than misrepresented as CSS breakpoints.

## Evidence boundary

v1.2 source and screenshot review is accepted for shipping. Publish succeeded: `dist/1.2.0/Contador.exe` reports version 1.2.0.0. All 35 runtime checks passed with copies of actual histories, and native overlay snapshots passed. Current review captures are in the task workspace `work/v12-review`; earlier captures remain in `.impeccable/review`. No browser detector applies to this native Windows surface.

## Fechas del contador (1.2.7)

## Mapa interactivo nativo (1.2.7)

Extensión ordinaria elegida por el usuario: mapa propio WPF con Fortnite.GG como referencia de interacción. Conserva navy, rojo, marfil, dorado y Segoe UI. No cambia tokens, DESIGN.md ni su sidecar. Sustituye el diagrama de 375 píxeles por una imagen local de 2048 píxeles de Fortnite-API e iconos vectoriales propios; atribución en docs/MAPA-INTERACTIVO-1.2.7.md.

Barra adaptable, panel lateral desplazable de 230 unidades y mapa flexible. Zoom 100–800 %, arrastre, flechas, Inicio, ocultar capas y ventana ampliada. La agrupación fusiona marcadores próximos y comparte centros entre dibujo y selección. La lista filtrada y el selector de puntos coincidentes ofrecen acceso alternativo al lienzo. El recuento expresa puntos con los filtros, no puntos dentro del encuadre.

Instantánea comunitaria crossbones-43.10-20261113: 2.145 apariciones en nueve categorías, más 16 lugares. Fuente lifecrisis.life/loot, consultada el 13/11/2026. No se verificó cada punto en partida; no garantiza botín, premios ni probabilidades. No hay actualización automática ni lectura de posiciones del jugador.

Ruta coral numerada de hasta 100 paradas manuales, editables y reversibles. Deshacer conserva 30 acciones. Revisado, Vacío y Pendiente son marcas manuales. Capas, ruta y marcas se guardan por catálogo en un archivo local independiente del historial; errores visibles y archivo dañado preservado. Edición desactivada al reabrir. Las líneas indican orden de visita, no navegación calculada ni predicción.

Evidencia: capturas WPF actuales en outputs/mapa-interactivo/renders del espacio de la tarea, con anchos normal/reducido, zoom, ruta y filtros vacíos; informe con datos ficticios identificados. Revisión final ship limitada a los dos ajustes puntuados: separación de grupos y etiqueta de recuento, ambos resueltos. Suite general de almacenamiento bloqueada por UnauthorizedAccess del entorno; no se considera aprobada. No corresponde detector web.

Verificación final: 40 comprobaciones del mapa y 15 de controles WPF aprobadas; publicación autocontenida win-x64 correcta. El ejecutable nuevo queda en outputs del espacio de trabajo: la copia a dist fue denegada por el entorno. No se sustituyó ni cerró el contador abierto.

## Terreno HD opcional (1.2.7)

Actualización solicitada por el usuario: HD inicia activado por defecto. El control permite apagarlo y reactivarlo, y se conserva el respaldo integrado. Dieciocho pruebas WPF aprobadas con esta nueva condición inicial. Esta petición sustituye el estado inicial desactivado descrito durante la revisión original siguiente; no cambia el sistema visual.

Extensión ordinaria del mapa propio: conserva la composición, los controles WPF y el sistema visual existente. La casilla nativa «Terreno HD · Internet» inicia desactivada. Al activarla, el lienzo dibuja piezas WebP públicas de Fortnite.GG del mapa 43, de 256 × 256 píxeles, en niveles 3–7 elegidos según escala y DPI. La atribución al proveedor permanece visible; no se incrusta su sitio ni su interfaz. El zoom llega hasta 3200 %. El acercamiento separa grupos progresivamente; puntos coincidentes conservan su selector, sin inventar plantas o alturas.

MapTiles.cs define la pirámide y el encuadre; MapTileCache.cs limita la carga a cuatro peticiones simultáneas, ocho segundos por petición, dos minutos antes de reintentar una pieza fallida, 96 piezas en memoria y 128 MB en disco. Desactivar HD o descargar el control cancela las peticiones. El estado visible explica carga, caché o respaldo. Cuando falta una pieza, la red o el decodificador WebP, el fondo integrado permanece disponible; puede coexistir temporalmente con piezas de mayor detalle. Solo se solicitan imágenes: no se envían credenciales, historial, repeticiones ni estadísticas.

Evidencia: 48 comprobaciones del motor, 18 de WPF y cuatro de caché WebP aprobadas. Una pieza real 6/29/34 se verificó en una caché aislada; los renders en outputs/mapa-hd/renders del espacio de la tarea muestran ese alcance parcial. No se verificó la disponibilidad de todas las piezas ni una sesión HD completa desde el cliente. docs/MAPA-HD-1.2.7.md conserva estos límites. Disposición final: ship para la extensión opcional con respaldo local; excluye disponibilidad completa del proveedor. No corresponde detector web. No se cambian los tokens ni se repara la deriva anterior del sistema.

## Comportamiento del filtro de fechas

Ajustes includes native DatePicker controls for inclusive local start/end dates, Apply, a 01/11/2026 open-end preset and Clear. Controls wrap at the minimum window width; inline errors describe invalid ranges. The shared status panel names the applied period on Resumen, Historial and Ajustes; Informe carries its own period, while editorial routes are independent of replay filters. History is filtered without deleting stored matches. All-date remains the default; filters persist and scope totals, profiles, CSV, report and overlays.
