# Verificación de Contador 1.2

- Compilación Windows x64 autónoma: ejecutable dist/1.2.0/Contador.exe.
- 35 comprobaciones correctas con una copia del historial real: cabeceras, perfiles, persistencia, importación, respaldo, detección de partida, retirada del aviso al finalizar y exclusión de repeticiones abandonadas.
- Sesión comprobada con historial anterior, importaciones, repeticiones antiguas y varias partidas nuevas. Última partida y sesión se calculan por separado.
- Capturas nativas del overlay en espera y en partida, con etiquetas de última partida, sesión y tesoros.
- Launcher renderizado en tamaño normal y compacto, historial y ajustes. Moneda aportada por el usuario embebida como recurso WPF.
- Revisión visual independiente: ship para el código y las capturas; sin hallazgos materiales.
- La detección se basa en cabeceras de repeticiones escritas recientemente por Fortnite y se consulta cada dos segundos. No se validó durante una partida real en esta sesión de trabajo. El recuento final sigue dependiendo de terminar la repetición.
- Persisten avisos heredados y de consulta de vulnerabilidades por falta de acceso a nuget.org; no se alteró el analizador de cofres.
