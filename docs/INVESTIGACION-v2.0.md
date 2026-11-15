# Investigación y decisiones de Contador 2.0

Consulta: 14 de noviembre de 2026. Se revisaron cinco proyectos públicos y sus rutas de análisis. No se encontró evidencia de un lector que garantice recuperar todas las aperturas de todo el equipo desde una sola repetición local. Revisar el código no equivale a validar sus resultados contra una grabación manual completa.

## Proyectos revisados

| Proyecto y revisión fijada | Arquitectura y hallazgo | Decisión local |
|---|---|---|
| [LED-N/Fortnite-ChestCounter](https://github.com/LED-N/Fortnite-ChestCounter/tree/c4ca1400ff1f79dbee4ea3ecb3f8eb72c8e6bb82) · MIT | Lector C# → ReplayAnalyzer → modelos/Store → Tracker → GUI, overlay e informe. ReplayAnalyzer relaciona pawns y jugadores, identidad estable del contenedor y ciclos de relleno. Su versión reciente aplica la misma elegibilidad de snapshots estables a recuentos personales y de compañeros. | Conservar el lector base y sus correcciones locales de seguridad; unificar la política personal/equipo y extraer el registro de ciclos para poder probarlo de forma aislada. |
| [b0ulbix/fortnite-chest-tracker](https://github.com/b0ulbix/fortnite-chest-tracker/tree/7fa8c69376d5cfff384deae948519da5b76f9eb2) · MIT | Vigilancia y presentación en Python; lector C# y salida estructurada. Program.cs distingue búsqueda por pawn del dueño de aproximaciones espaciales para otros casos. | Mantener separadas identidad observada y atribución aproximada. No convertir cercanía espacial en una apertura personal confirmada. |
| [Tanron009/fortnite-chest-scan](https://github.com/Tanron009/fortnite-chest-scan/tree/727203d2be24091c6516df5bed5013dccdb07362) | Lector Go; container.go enlaza SearchingPawn con PlayerState y el grabador. analyze.go puede devolver resultado parcial junto con un error. Un conjunto de actores ya vistos puede impedir nuevos ciclos del mismo actor. | Estudiado como referencia; sin licencia identificada en el árbol consultado, no se copió código. No adoptar la deduplicación permanente por actor ni presentar una lectura fallida como un cero válido. |
| [Yakfal/fortnite-chest-report](https://github.com/Yakfal/fortnite-chest-report/tree/6f97755055f2ecb816afa5c6786d015ac7659aaa) | fc_core.py coordina un lector ejecutable externo y genera informes; algunas rutas distinguen aperturas propias, ajenas y destruidas. Tiene deduplicación espacial, que puede colapsar contenedores próximos. | No se ejecutó ni se acreditó la implementación del lector binario. Sin licencia identificada en el árbol consultado, no se copió código. No usar solamente una celda espacial como identidad del cofre. |
| [lvortexxx/ChestTokenAssistant](https://github.com/lvortexxx/ChestTokenAssistant/tree/7eab883267a6b2c546a910ceaf0db7b1706b4905) · MIT | Team.cs combina perspectivas por sesión y jugador: sustituye el mínimo observado de un compañero por el recuento de su propia fuente. Incluye resúmenes externos y una alternativa por nombre. | Adoptar el enfoque de sustitución, con implementación propia y requisitos más estrictos: archivos .replay leídos localmente, sesión verificada, Epic ID y pertenencia conocida al equipo. Sin coincidencia solo por nombre, ni importación de resúmenes JSON/CSV no verificados. |

Los avisos de licencia del lector original se mantienen en vendor. Las implementaciones Go y Python sin licencia identificada solo se utilizaron para comparar arquitecturas. No se incorporaron binarios ajenos ni servicios nuevos.

## Cambios concretos

- **ContainerInteractionLedger:** concentra transiciones por contenedor y generación de canal. Una notificación repetida o una reentrada del mismo contenedor no se suma como una nueva apertura por sí sola. Un estado cerrado observado rearma el ciclo. La destrucción de un cofre cerrado se procesa aunque el mismo paquete diga que todavía no fue buscado.
- **Estado replicado:** SearchingPawn no es un argumento enviado obligatoriamente en cada evento. Un campo omitido conserva su valor; un cero explícito o un canal nuevo lo limpia. Se permite completar una atribución inicialmente desconocida dentro de cinco segundos, en el mismo canal y ciclo; ese margen es una política conservadora local, no una garantía del protocolo.
- **Identidad tardía:** las relaciones pawn/jugador se resuelven al terminar de leer. Si no se identifica al jugador solicitado, el análisis falla explícitamente en vez de guardar un cero como si fuera una partida correcta.
- **Regla común:** personal y equipo aplican el mismo filtro de snapshot e identidad estable. Las aperturas sin autor se conservan como diagnóstico, no se asignan por proximidad al jugador.
- **TeamReplayReconciliation:** cada fuente complementaria debe pertenecer a la misma sesión verificada y a un compañero conocido. Su aportación sustituye la observada, no se suma dos veces. Una fuente contradictoria inferior no reduce el mínimo; se señala el conflicto. No cambia el historial personal ni atribuye nuevas recompensas.
- **Almacenamiento mínimo:** los archivos compartidos no se copian ni se suben. Se guarda un resumen local de identidad, partida y cofres. Importación limitada a 20 archivos por operación y 1.000 resúmenes. La lectura usa los límites del lector existente. Una operación fallida no sustituye los datos previos.
- **Compatibilidad:** se mantiene la migración con respaldo data.before-analysis-20.json. Se vuelven a leer solo las repeticiones todavía disponibles; los registros antiguos sin archivo no se inventan de nuevo.

## Límites y evidencia pendiente

En upstream siguen existiendo reportes de diferencias, por ejemplo [issue 37](https://github.com/LED-N/Fortnite-ChestCounter/issues/37) y [issue 20](https://github.com/LED-N/Fortnite-ChestCounter/issues/20). No hay base para anunciar que los bugs de las repeticiones estén solucionados universalmente.

Los snapshots iniciales con identidad estable se admiten según la política del lector base; no representan necesariamente una transición observada en ese instante. La atribución ausente, eventos no replicados, destrucciones con vehículos y cambios futuros de Fortnite siguen siendo límites. Varias perspectivas mejoran cobertura, pero tampoco prueban una grabación completa del servidor. Los premios recibidos y el origen confirmado de esos premios siguen siendo conceptos distintos.

La unión de fuentes tiene pruebas sintéticas de sesión, identidad, duplicación y conflicto. Todavía falta contrastarla con archivos de varios compañeros de una misma partida y un conteo manual grabado. No se ha simulado que esas pruebas equivalgan a una validación de campo completa.

## Pruebas realizadas

- 250 comprobaciones del motor, incluidas 25 del registro de ciclos y conciliación: campos parciales, rellenos, reentrada de canal, destrucción, atribución tardía, duplicados, fuentes equivocadas y conflictos.
- Tres repeticiones locales leídas en modo de solo lectura. Cofres personales antes/después: 11/11, 39/39 y 35/35. Aperturas detectadas de compañeros: 6/6, 21/22 y 17/25. Son controles de compatibilidad con el registro anterior, no verdad terreno validada manualmente.
- No se modificó el historial real para realizar estas comprobaciones. No se publican archivos .replay, identificadores de cuentas ni rutas personales en los documentos del repositorio.

## Revisión del equipo y del launcher · 15/11/2026

Se contrastaron otras implementaciones públicas, fijando sus revisiones:

- [andruhan123YT/fortnite-chest-counter](https://github.com/andruhan123YT/fortnite-chest-counter/tree/34f939e85f4558fb717d9e8d390357d8f65fed8f) (Apache-2.0): pantalla de fuentes de escuadrón y conciliación del máximo observado por jugador entre perspectivas. Referencia arquitectónica; no se copió código Rust.
- [lvortexxx/ChestTokenAssistant](https://github.com/lvortexxx/ChestTokenAssistant/tree/7eab883267a6b2c546a910ceaf0db7b1706b4905) (MIT): distingue la propia perspectiva del compañero de lo observado desde otro jugador. No se adoptó su importación de resúmenes JSON/CSV ni la vinculación por nombre.
- [craftznora/Plunder-Count](https://github.com/craftznora/Plunder-Count/tree/0f63e56b07372d33d536b0a56145ca1660e6d952) (MIT): atribución por jugador/equipo sobre el lector base. No demuestra recuperación de acciones que nunca se replicaron.

### Corrección aplicada

El total conciliado y las filas por jugador ahora comparten `TeamReplayReconciliation`. Antes una fila podía seguir mostrando la observación local aunque el total ya usara una perspectiva complementaria. Para cada compañero conocido se conserva la mejor cifra disponible entre las fuentes de la misma sesión verificada, sin sumar dos veces una misma aportación. Se distingue su propia repetición de una observación desde otro miembro; esta última sigue siendo parcial. No se enlazan fuentes solo por nombre, proximidad temporal o nombre de archivo.

Una fuente inferior no reduce el mínimo previo. Las discrepancias permanecen identificadas. Los resúmenes locales incluyen el desglose necesario por jugador; no contienen capturas, contraseñas ni archivos de repetición copiados. Las cifras personales y las recompensas no cambian por importar fuentes del equipo.

### Uso y comprobación

Equipo permite escoger partida, importar archivos `.replay` finalizados y retirar cada fuente sin borrar su archivo. Un archivo no apto ya no impide aprovechar los demás del lote. El importador muestra el resultado y conserva los límites existentes.

Se aprobaron 285 comprobaciones del motor y 16 del conjunto de seis pantallas con datos ficticios. Estas pruebas comprueban conciliación, duplicación, selección y presentación; no sustituyen una comparación manual de varios compañeros en una misma partida. Ninguna de las fuentes revisadas justifica prometer un total de servidor exacto desde una sola repetición local.
