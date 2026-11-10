# Verificación de Contador 1.2.1

- Compilación Windows x64 autónoma: ejecutable dist/1.2.1/Contador.exe.
- 35 comprobaciones correctas con una copia del historial real: cabeceras, perfiles, persistencia, importación, respaldo, detección de partida, retirada del aviso al finalizar y exclusión de repeticiones abandonadas.
- Sesión comprobada con historial anterior, importaciones, repeticiones antiguas y varias partidas nuevas. Última partida y sesión se calculan por separado.
- Capturas nativas del overlay en espera y en partida, con etiquetas de última partida, sesión y tesoros.
- Launcher renderizado en tamaño normal y compacto, historial y ajustes. Moneda aportada por el usuario embebida como recurso WPF.
- Revisión visual independiente: ship para el código y las capturas; sin hallazgos materiales.
- La detección se basa en cabeceras de repeticiones escritas recientemente por Fortnite y se consulta cada dos segundos. No se validó durante una partida real en esta sesión de trabajo. El recuento final sigue dependiendo de terminar la repetición.
- Persisten avisos heredados y de consulta de vulnerabilidades por falta de acceso a nuget.org; no se alteró el analizador de cofres.

## Actualización 1.2.1

La moneda se usa como icono multirresolución del ejecutable y ventana (16–256 px) y reemplaza el cofre del overlay mediante dibujo nativo con transparencia. Todos los recursos están embebidos. Pasaron 43 comprobaciones en el modo de captura, incluidas las siete resoluciones del ICO, el dibujo real de la moneda y capturas de ambos estados del overlay. El README documenta procesamiento local y ausencia de inicio de sesión o credenciales; no afirma aprobación de Epic ni inmunidad a sanciones.
