# Contador 1.2.5 — Recuento más rápido

- Revisión de repeticiones cada segundo en lugar de cada ocho segundos.
- Estabilización de un segundo para archivos finalizados, con comprobación de tamaño y fecha de escritura.
- Launcher y overlays se actualizan antes de generar el informe.
- El análisis ya no espera una compactación forzada de memoria y usa prioridad por debajo de normal, en lugar de la más baja.
- Se reintentan las repeticiones que cambian durante la lectura y se evita regenerar el informe cuando no hay cambios.

La mejora reduce esperas internas. El tiempo total sigue dependiendo de que Fortnite termine la repetición y escriba la clave, del tamaño del archivo y del equipo; no se promete un recuento instantáneo.
