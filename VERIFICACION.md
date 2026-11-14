# Verificación de Contador

## Segundo plano y bandeja · 1.3.1

- Ocho comprobaciones del flujo de segundo plano: cancelar, ocultar sin guardar ni detener, dispatcher activo mientras está oculto, restauración, ocultación deshabilitada si no hay icono y salida explícita sin repetir la pregunta ni el guardado.
- Tres comprobaciones de cierre: inmediato, análisis pendiente y error de guardado.
- Las pruebas nativas de registro de icono fueron bloqueadas por Win32 5 en el entorno de ejecución. Las capturas reales aportadas por el usuario muestran la moneda, el menú Abrir Contador/Salir y la elección al cerrar funcionando en su escritorio. No se ha simulado reiniciar su Explorador ni cerrar la sesión de Windows.
- Se conserva el bloqueo de instancia única y no se activa el inicio automático con Windows.
- Los resultados de estas pruebas no acreditan la suite general de almacenamiento, previamente bloqueada por permisos del entorno.

## Versión del ejecutable · 1.3.0

Proyecto actualizado a 1.3.0. La etiqueta de la ventana y el título del mapa usan AppVersion, derivada de la versión del ensamblado, en lugar de literales separados. Se comprobó por reflexión la etiqueta v1.3 y la versión 1.3.0.0; publicación autocontenida win-x64 completada. Pasaron 18 pruebas WPF y ocho de instancia única. La salida de esta versión está en dist/1.3.0/Contador.exe; el ejecutable histórico de dist/1.2.7 sigue siendo una compilación anterior.

## HD activado por defecto · 1.2.7

Por petición del usuario, Terreno HD inicia activado. Se mantienen el control para desactivarlo y el respaldo integrado. Las 18 comprobaciones WPF pasan con la nueva condición inicial y verifican desactivación y reactivación. Las pruebas y capturas aisladas bloquean la red para evitar descargas durante su ejecución. Esta modificación sustituye la condición inicial apagada documentada en las verificaciones históricas siguientes.

## Inicio e instancia única · 1.2.7

Ocho pruebas aisladas de mutex y señal de activación aprobadas: objeto existente sin propietario, comprobación repetida, liberación al salir, instancia realmente ocupada, activación de la principal, secundarias que no liberan el bloqueo ajeno y recuperación tras terminar el hilo propietario. Dieciocho pruebas del mapa aprobadas. El cierre y guardado general volvió a quedar bloqueado por permisos File.Move del entorno, por lo que no se declara aprobado. No se ejecutó la aplicación completa sobre el historial personal para esta prueba.

El arranque comprueba propiedad efectiva del mutex, libera explícitamente su bloqueo al salir y controla los errores al crear la ventana para evitar permanecer abierto sin ventana. Un segundo inicio notifica a la instancia existente para restaurarla; también intenta restaurar la ventana de versiones anteriores. No termina procesos ajenos ni ignora un bloqueo que siga teniendo propietario.

## Terreno HD opcional · 1.2.7

48 comprobaciones del mapa, 18 de controles WPF y cuatro de caché con una pieza WebP real aprobadas. Construcción nativa correcta. La opción HD inicia apagada, dispone de atribución, zoom hasta 3200 %, carga por niveles y respaldo integrado. Capturas reales en el espacio de la tarea: outputs/mapa-hd/renders; la captura HD utiliza una sola pieza local y muestra cobertura incompleta. La revisión visual aprueba esta extensión opcional, sin certificar la disponibilidad del servidor externo. No se validó una sesión HD completa por Internet. Procedencia y límites en docs/MAPA-HD-1.2.7.md.

- Compilación Windows x64 autónoma: ejecutable dist/1.2.5/Contador.exe.
- 35 comprobaciones correctas con una copia del historial real: cabeceras, perfiles, persistencia, importación, respaldo, detección de partida, retirada del aviso al finalizar y exclusión de repeticiones abandonadas.
- Sesión comprobada con historial anterior, importaciones, repeticiones antiguas y varias partidas nuevas. Última partida y sesión se calculan por separado.
- Capturas nativas del overlay en espera y en partida, con etiquetas de última partida, sesión y tesoros.
- Launcher renderizado en tamaño normal y compacto, historial y ajustes. Moneda aportada por el usuario embebida como recurso WPF.
- Revisión visual independiente: ship para el código y las capturas; sin hallazgos materiales.
- La detección se basa en cabeceras de repeticiones escritas recientemente por Fortnite y se consulta cada dos segundos. No se validó durante una partida real en esta sesión de trabajo. El recuento final sigue dependiendo de terminar la repetición.
- Persisten avisos heredados y de consulta de vulnerabilidades por falta de acceso a nuget.org; no se alteró el analizador de cofres.

## Actualización 1.2.1

La moneda se usa como icono multirresolución del ejecutable y ventana (16–256 px) y reemplaza el cofre del overlay mediante dibujo nativo con transparencia. Todos los recursos están embebidos. Pasaron 43 comprobaciones en el modo de captura, incluidas las siete resoluciones del ICO, el dibujo real de la moneda y capturas de ambos estados del overlay. El README documenta procesamiento local y ausencia de inicio de sesión o credenciales; no afirma aprobación de Epic ni inmunidad a sanciones.

## Cierre corregido en 1.2.2

Tres pruebas con ventanas WPF nativas pasaron: cierre inmediato, cierre mientras hay un análisis pendiente (incluyendo clics repetidos) y fallo de guardado. El controlador aplaza la persistencia y el cierre final mediante el Dispatcher, de modo que nunca vuelve a llamar Close ni muestra diálogos dentro del primer evento Closing.

## Overlay unificado en 1.2.3

La fuente de OBS utiliza ObsHud y la misma vista inmutable que NativeOverlay. Pasaron 46 comprobaciones del motor/HUD, incluidas transparencia, moneda, etiquetas, datos locales, estado de partida y protección frente a la regeneración del informe. Se ejecutó también el JavaScript generado con un documento simulado: actualizó cifras y estado, y solicitó exclusivamente el archivo local cada dos segundos.

No se verificó visualmente dentro de OBS. Los navegadores de pruebas locales fallaron al iniciarse y la herramienta de navegador bloqueó la apertura de archivos locales por política. La compilación y las pruebas de generación/actualización pasaron; queda comprobar la apariencia en la fuente Navegador de OBS.

## Reporte renovado en 1.2.4

Se actualizó el CSS del reporte a la paleta y tipografía del launcher y se integró la moneda en una URI de datos. Se añadieron comprobaciones de colores, imagen embebida, ausencia del tema claro anterior y conservación de gráficos y tablas. La herramienta de navegador de esta sesión no permite abrir archivos locales; no se certifica una inspección visual del reporte en ese navegador.

## Actualización rápida en 1.2.5

Pasaron 63 comprobaciones del motor y 3 pruebas WPF de cierre. Las pruebas deterministas verifican dos snapshots idénticos y un segundo de estabilidad, reinicio de espera ante cambios de tamaño o fecha y rechazo de confirmación si el archivo cambia o desaparece durante el análisis.

El sondeo pasa de ocho a un segundo. Launcher y overlays reciben las cifras antes de generar el informe; se elimina el informe repetido sin cambios y la compactación forzada previa a publicar resultados. No se midió la latencia de extremo a extremo durante una partida real en esta sesión; no se promete un tiempo fijo de actualización.

## Corrección de cuentas · 1.2.6

La prueba WPF utiliza un ComboBox real con dos cuentas de igual nombre y distintos Epic ID. Mantiene ocupado el bloqueo del analizador, elige la segunda cuenta, simula una actualización y comprueba que la elección pendiente se conserva. Tras liberar el bloqueo verifica el perfil guardado y el total separado; también permite volver al primero y desactiva la lista vacía. Pasaron las cuatro comprobaciones WPF y las 61 comprobaciones básicas del motor. No se ha validado todavía con las partidas de los usuarios que reportaron el problema.

## Período por fechas · 1.2.7

Pasaron 77 comprobaciones del motor y las cuatro comprobaciones WPF. Las nuevas pruebas cubren inicio y fin inclusivos, conversión UTC a fecha local, UTC heredada sin Kind, fecha de análisis como respaldo, rango invertido, configuración anterior sin filtro, persistencia, cuenta separada, perfiles sin partidas, historial conservado y coherencia del CSV, informe y overlay. Los calendarios y el mensaje de error se revisaron visualmente con XAML renderizado a 1120×740 y 860×600; las capturas usan estado de demostración. No se ha validado el filtro con historiales de los usuarios que reportaron el problema.

## Mapa interactivo e informe nativo · compilación 1.2.7

- Publicación autocontenida Windows x64 completada, con mapa y catálogo integrados.
- 40 comprobaciones del mapa aprobadas: catálogo, coordenadas, agrupación sin pérdida ni colisiones, límites de zoom, edición, deshacer y formato del plan.
- 15 comprobaciones de controles WPF aprobadas.
- Once vistas WPF renderizadas; la selección del README procede de esas capturas. El informe utiliza datos ficticios identificados.
- Revisión final de dos ajustes: separación de grupos y etiqueta del total filtrado, ambos resueltos.
- La suite general de almacenamiento y las pruebas anteriores de perfil quedaron bloqueadas por UnauthorizedAccess del entorno en esa ejecución; no se declaran aprobadas.
- No se validaron todas las ubicaciones comunitarias en partidas reales. Son posibles apariciones de una instantánea 43.10, no detección de botín ni predicción.
- La huella debe recalcularse tras cada compilación o firma. Usa el archivo Contador.exe.sha256 que acompaña al ejecutable, no una huella histórica del README.

## Corrección de bóvedas · 1.2.7

43 comprobaciones del mapa y 15 de controles WPF aprobadas. Doce capturas nativas generadas, incluida capa aislada de nueve bóvedas. Alineación visual contrastada con la referencia aportada y grupos independientes de cofres raros. Corregida recursión de notificaciones de selección detectada durante esta ejecución. No se certifica ubicación de entradas ni precisión de las demás capas en una partida real.
