# Contador 2.2.1 — Rutas más fáciles y cambio de cuenta

## Cambios

- Se elimina el límite de cien paradas al añadir, cargar y guardar rutas.
- Doble clic en el mapa inicia una ruta; después basta un clic por parada.
  Arrastra una parada para moverla y pulsa Esc para terminar la edición.
- Reiniciar ruta vacía las paradas y Deshacer permite recuperarlas.
- Añadir cofres de este lugar incorpora sus referencias por proximidad, evitando
  repetir posiciones que ya estén en la ruta.
- Se incorporan dos sugerencias comunitarias: un circuito desde Lantern Landing
  y una búsqueda en Cursed Cavern. Las fuentes y límites están en cada guía.
- El selector de cuenta utiliza la elección del usuario directamente y conserva
  las cifras separadas durante el análisis de repeticiones.

## Alcance

Las rutas son planificación local. No calculan caminos transitables, plantas,
accesos ni cofres disponibles durante la partida. Las sugerencias comunitarias
son orientativas; no garantizan premios. El catálogo sigue siendo la instantánea
43.10. El conteo visual y el grupal continúan siendo experimentales.

## Actualizar

Cierra Contador desde la bandeja → Salir y abre el nuevo Contador.exe.
No borres %APPDATA%\ContadorModern: conserva el historial y las preferencias.
La compilación está en dist\2.2.1\Contador.exe. La instalación es manual.

## Validación

La revisión funcional pasó 466 comprobaciones de motor y escritorio, incluyendo
rutas largas, recarga, reinicio, deshacer y selección de cuentas durante análisis.
El cambio de versión se compila desde el mismo código como 2.2.1.

## Archivo para la Release

- Etiqueta: `v2.2.1`.
- Título: **Contador 2.2.1 — Rutas más fáciles y cambio de cuenta**.
- Adjunto: `dist/2.2.1/Contador.exe`.
- Versión del archivo de Windows: `2.2.1.0`; versión visible: `2.2.1`.
- SHA-256 del ejecutable preparado:

```text
8d462b83e34c8690f0cfa6ba0905c035c2b7c1eae29511b2991dcf0916c68a0b
```

Esta huella identifica esta compilación, no acredita su seguridad por sí sola.
El ejecutable no tiene firma digital de editor verificado. Windows puede mostrar
SmartScreen. No es necesario desactivar el antivirus ni crear exclusiones.

Publicar el código no adjunta automáticamente el ejecutable: hay que añadirlo a
la Release para que el botón de descarga directa entregue la nueva versión.
