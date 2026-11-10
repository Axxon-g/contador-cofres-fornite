# Contador 1.2.2 — Cierre corregido

- Corrige el mensaje de WPF al cerrar la app sin un análisis pendiente.
- Un solo clic inicia el guardado y cierra la ventana; las pulsaciones repetidas mientras termina un análisis no duplican el guardado.
- La app sigue esperando a que termine el análisis actual antes de guardar el historial.
- Nuevas pruebas con ventanas WPF reales para cierre inmediato, cierre con análisis pendiente y error de guardado.

Descarga Contador.exe, cierra la versión anterior y abre la nueva. Se conserva el historial local.
