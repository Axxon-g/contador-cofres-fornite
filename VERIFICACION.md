# Verificación de Contador 1.2.1

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
