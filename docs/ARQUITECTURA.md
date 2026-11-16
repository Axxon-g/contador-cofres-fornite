# Arquitectura

## Lectura visual opcional · experimental

VisualChestCounter y VisualRoster contienen reglas puras, independientes del historial. VisualCaptureSession controla una autorización de ejecución y usa FortniteFrameSource para copiar únicamente regiones del HUD de Fortnite en primer plano. Windows.Media.Ocr procesa los píxeles en memoria. MainWindow.Live coordina permiso, pausa, cancelación y presentación. El permiso no se persiste. La estimación se publica como VisualEstimate separada en el overlay y nunca modifica MatchRecord, el historial ni las recompensas. Los nombres visuales no se utilizan como identidad de cuenta.

La compilación completa con Windows SDK y las pruebas OCR sobre imágenes generadas están verificadas; la calibración con una partida real sigue pendiente. Ver docs/LECTURA-VISUAL-2.0.md. VisualChestCounter concilia cantidades parciales entre lecturas y no acepta una cifra alta aislada. El trabajo OCR se ejecuta fuera del hilo de interfaz y vuelve a comprobar partida, permiso y pausa antes de cada recorte.

## Responsabilidades

- **Contador (WPF):** ventanas, navegación, selección de cuenta, bandeja y cierre. `MainWindow` coordina operaciones mediante una exclusión compartida; no decodifica repeticiones.
- **Contador.Engine:** importación del historial, filtros, sesión, cálculo del equipo, informes y mapa. `TeamAccounting` concentra el cálculo parcial del equipo. `OverlayPublisher` conserva una instantánea inmutable y publica únicamente cambios reales o una activación forzada.
- **Motor heredado seleccionado:** almacenamiento, seguimiento de archivos, análisis de partidas y dibujo nativo. El proyecto Engine enumera explícitamente los archivos necesarios; no compila el programa de consola, el actualizador, el lanzador de espera ni la bandeja heredada. Los originales se conservan en vendor para mantener su procedencia y licencias. ConsoleUi permanece porque Tracker utiliza su progreso de consola.
- **Bibliotecas de lectura:** FortniteReplayReader → Unreal.Core → OozSharp. Validan tamaños, reservas y progreso antes de procesar datos externos.
- **Comprobaciones:** Contador.Checks cubre reglas, formatos, almacenamiento y límites de lectura; Contador.DesktopChecks cubre la interfaz y su ciclo de vida.

## Flujo de datos

1. El seguimiento espera una repetición finalizada y estable.
2. El lector obtiene registros de la partida bajo límites de recursos.
3. El almacenamiento conserva el historial; los filtros seleccionan cuenta y período sin borrar partidas.
4. Dashboard y TeamAccounting preparan los datos de presentación.
5. OverlayPublisher entrega la misma instantánea al HUD nativo y a OBS. Durante el análisis, la presencia se actualiza a partir de la instantánea anterior, sin leer colecciones que estén cambiando.
6. Los informes se regeneran cuando hay cambios. El sondeo sin novedades no anuncia un análisis ni reescribe el HTML del overlay.

## Límites que deben conservarse

El apartado Estadísticas utiliza `PlayerStatisticsViewModel` → `PlayerStatisticsClient` → servicio fijo HTTPS de AXXON. La consulta inicial requiere pulsar Consultar; la cuenta local es opcional. Solo salen nombre/ID público y filtros, sin historial, capturas ni replays. El cliente no almacena claves, no envía Authorization/cookies y no sigue redirecciones. El JSON remoto se limita a 256 KiB y profundidad 12, valida identidad por ID y conserva datos ausentes. Cancelación y generación de solicitud impiden presentar resultados de un perfil anterior. La clave del proveedor y los límites compartidos pertenecen al servidor web, no al ejecutable.

- Los datos personales y el registro parcial del equipo no se sustituyen entre sí.
- Un dato de equipo ausente no equivale a cero; las atribuciones estimadas no son confirmaciones.
- El historial se guarda en AppData, separado del ejecutable y de las rutas manuales del mapa.
- Los cambios de datos y configuración se serializan; la presentación de WPF se actualiza en su hilo.
- Las exportaciones usan CsvText para neutralizar fórmulas y escapar celdas.
- dist contiene artefactos de distribución ignorados por Git. No se publican historiales, preferencias ni claves.

La limpieza de 1.3.2 delimita dependencias y extrae la publicación del overlay; no reemplaza el lector heredado ni elimina sus fuentes necesarias.
## Registro de interacciones y perspectivas · 2.0

ReplayAnalyzer traduce los paquetes del lector a ContainerInteractionLedger. Este conserva el estado replicado por canal y el ciclo consumido por contenedor; emite interacciones que se resuelven contra el grafo de identidad al terminar la lectura. La misma elegibilidad se utiliza para el dueño y sus compañeros.

TeamReplayReconciliation verifica sesión, cuenta y pertenencia al equipo antes de sustituir una contribución observada. No muta MatchRecord ni utiliza nombres como identidad. TeamAccounting, Dashboard.Team, las filas de la última partida y la instantánea del overlay consumen esa misma conciliación. MainWindow.TeamReplays coordina selección, lectura fuera del hilo de interfaz y guardado bajo la exclusión compartida. El historial guarda resúmenes mínimos separados en TeamReplaySources; HistoryImporter los conserva.

La exactitud personal no se infiere de proximidad espacial. Los datos ausentes del equipo siguen siendo desconocidos, no ceros confirmados. Las fuentes complementarias no se añaden como partidas personales. Investigación, procedencia y limitaciones: [INVESTIGACION-v2.0.md](INVESTIGACION-v2.0.md).

## Contador 2.2

SummaryView y TreasureOddsView encapsulan la presentación sin acceder por su cuenta
al historial. TreasureProbability y MissingMatchEstimate son cálculos puros; las
simulaciones no se guardan como aperturas. ReplayRetryPolicy, ReplayArchive y
ReplayDiagnostics delimitan recuperación, copias opcionales y diagnóstico reducido.
ReleaseChecker solo consulta metadatos públicos de un destino fijo; la ventana
controla cancelación, mensajes y apertura manual del enlace oficial. No hay un
instalador automático ni se incorpora el actualizador heredado del CLI.

[Detalles de recuperación](ADAPTACION-2.2.md) · [Interfaz](DISENO-2.2.md).
