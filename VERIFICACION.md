# Verificación de Contador 1.2.1

- Compilación Windows x64 autónoma: ejecutable dist/1.2.3/Contador.exe.
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
