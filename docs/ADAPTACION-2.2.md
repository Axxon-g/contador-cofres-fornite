# Contador 2.2 — adaptación funcional

Referencia revisada el 16/11/2026:
[ayman-studio/fortnite-chest-tracker](https://github.com/ayman-studio/fortnite-chest-tracker).
Se adapta el comportamiento a la arquitectura existente, conservando WPF, el
historial de Contador, el mapa y sus controles de privacidad. No se sustituye el
historial por el formato de otro programa ni se ejecutan sus servicios remotos.

| Función de referencia | Contador 2.2 |
| --- | --- |
| Repetición finalizada y estable | Se conserva la estabilidad de tamaño/fecha y la comprobación de clave; nunca se cuenta un archivo que cambió durante el análisis. |
| Propietario por repetición y cuentas separadas | Se conserva la identificación del grabador y los perfiles, sin requerir coincidencia con un nombre escrito. |
| Cofres abiertos, destruidos y rellenados | Se conserva nuestro registro de interacciones, deduplicación por actor y evidencia de destrucción. |
| Fallos temporales | Hasta tres intentos, esperas de 2 y 4 minutos; los errores de contenido requieren reintento explícito. |
| Lector que supera el tiempo límite | Se solicita cancelación cooperativa en los bucles del lector. Un hilo aún activo bloquea nuevos lectores para evitar mezclar los hooks estáticos; el usuario puede reiniciar si no termina. |
| Reanálisis | Ajustes permite retirar solo marcas de error de archivos aún disponibles. Los éxitos y sesiones se conservan. |
| Archivo local | Opcional y apagado por defecto. Copias de nuevas repeticiones BR analizadas en `ReplayArchive` dentro de datos; nombre SHA-256, sin mover originales ni sobrescribir copias. No hay retención automática. |
| Probabilidades | Matemática local, tasas comunitarias explícitas, simulación separada y escenarios de Horas del tesoro. |
| Partidas anteriores | Escenario manual basado en la media del perfil/período, sin inventar aperturas ni sumarlas al historial. |
| Consulta por nombre | Cliente existente de AXXON: clave solo en servidor, tiempo límite, cancelación, tamaño acotado y caché de memoria. La API no proporciona cofres. |
| Overlay, OBS, bandeja e informe | Se mantienen los de Contador, sin sustituirlos por un servidor HTTP local. |
| Amigos por Internet | No se conecta con el servidor del proyecto de referencia. El equipo sigue usando las fuentes locales e importadas existentes, con cobertura parcial visible. |

## Límites que no se ocultan

La referencia usa un lector basado en el mismo commit de FortniteReplayDecompressor
que nuestro lector. No constituye una solución demostrada al error de jugador no
identificado. Su código no tiene nuestra barrera que rechaza guardar un cero cuando
faltan las asociaciones del personaje: esa protección se conserva. Para certificar
el caso reportado necesitamos una repetición afectada reciente y un control legible.

No se importan el actualizador, la instalación, el servidor de amigos, el servidor
HTTP local ni la interfaz web de terceros. Se evita añadir conexiones o cambios de
inicio de Windows ajenos al alcance de estas funciones.

## Organización

- `ReplayRetryPolicy`: decisión acotada de cuándo reintentar, sin acceso a archivos.
- `ReplayArchive`: copia local y deduplicación por contenido.
- `Tracker`: coordina estabilidad, lectura exclusiva, resultado, archivo y persistencia.
- `ReplayDiagnostics`: exporta metadatos y categorías sin texto libre del registro.
- `MissingMatchEstimate` y `TreasureProbability`: cálculos puros, sin persistencia.
- `MainWindow.ReplayTools`: acciones nativas de Ajustes, protegidas por el mismo semáforo.

La exportación de diagnóstico contiene tamaño, fecha, estado de grabación, categoría
de error y versiones. No contiene nombres de archivos, jugadores, ID de Epic, rutas,
mensajes de excepciones, claves ni datos de capturas. Se guarda solo donde el usuario
elige; no se transmite automáticamente.

## Créditos

Las ideas de recuperación, archivo, probabilidades y estimación se contrastaron con
`Tracker.cs`, `Estimates.cs` y `TreasureOdds.cs` de Ayman Studio. La implementación
de estos módulos es propia; no se copia su lector ni sus recursos. Se conserva una
copia de su aviso MIT en `vendor/LICENSE-ayman-studio.txt` como atribución de la referencia.
Las licencias de las dependencias ya incorporadas, incluido el descompresor, siguen
aplicándose; esta adaptación no las sustituye.

## Validación local

La preparación de 2.2 terminó con 365 comprobaciones del motor y 93 de escritorio,
458 en total. Véase [VERIFICACION.md](../VERIFICACION.md) para el desglose actual.
Una repetición reciente de control devolvió 24 cofres propios, 12 destruidos; es un
control de lectura, no una validación independiente de cada apertura ni del archivo
fallido reportado. El caso afectado y la exactitud de varias perspectivas siguen
pendientes de validación. Los avisos NU1900 en la publicación previa impidieron
comprobar vulnerabilidades de NuGet; no se presenta como auditoría de dependencias.
