# Arquitectura

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

- Los datos personales y el registro parcial del equipo no se sustituyen entre sí.
- Un dato de equipo ausente no equivale a cero; las atribuciones estimadas no son confirmaciones.
- El historial se guarda en AppData, separado del ejecutable y de las rutas manuales del mapa.
- Los cambios de datos y configuración se serializan; la presentación de WPF se actualiza en su hilo.
- Las exportaciones usan CsvText para neutralizar fórmulas y escapar celdas.
- dist contiene artefactos de distribución ignorados por Git. No se publican historiales, preferencias ni claves.

La limpieza de 1.3.2 delimita dependencias y extrae la publicación del overlay; no reemplaza el lector heredado ni elimina sus fuentes necesarias.