# Contador 2.0.1 — Estadísticas públicas y conexión corregida

## Novedades

- Nuevo apartado Estadísticas debajo de Resumen: victorias, partidas, eliminaciones, K/D, porcentaje de victoria, nivel del pase y modalidades disponibles.
- Búsqueda por nombre Epic, PlayStation o Xbox, con filtros de temporada/historial y tipo de control. Usar la cuenta local seleccionada es opcional.
- Corregida la dirección del servicio: consulta directa a `https://www.axxon.dev/api/fortnite-stats`, sin depender de redirecciones.
- El usuario no necesita clave de API, contraseña ni iniciar sesión. La clave del proveedor permanece en el servidor.
- Se conservan las funciones de 2.0, los datos locales y sus limitaciones experimentales.

## Privacidad y límites

La búsqueda envía el nombre o ID público y los filtros a AXXON y Fortnite-API. No se envían replays, capturas ni historial de cofres. La infraestructura de alojamiento puede registrar datos técnicos. El cliente no envía cookies ni cabeceras de autorización. Caché de memoria de hasta cinco minutos, límite de respuesta de 256 KiB, cancelación y espera máxima de quince segundos.

Esta API no ofrece cofres, recompensas ni Tríos. Los datos ausentes se muestran como —. El historial de cofres sigue dependiendo de repeticiones locales; el conteo grupal y el visual siguen siendo experimentales.

## Actualizar

1. Cierra Contador desde la bandeja → Salir.
2. Sustituye el ejecutable por Contador.exe de esta Release y vuelve a abrirlo.
3. Abre Estadísticas y consulta el nombre y la plataforma correspondientes.

No borres `%APPDATA%\ContadorModern`; conserva tu historial y ajustes. El ejecutable incluye .NET para Windows x64.

## Revisión previa a publicación

Revisión estática de la nueva integración y sus flujos de datos, más búsqueda de patrones de credenciales en archivos publicables e historial Git local. No se detectaron secretos ni fugas en los cambios revisados. Un identificador de sesión en una prueba es un dato de ejemplo, no una credencial.

La revisión no constituye una garantía absoluta de seguridad ni una auditoría completa de las dependencias o del servidor. Se excluyen de Git archivos de entorno, certificados privados, claves, replays y registros locales. El ejecutable continúa sin firma de editor verificado: SmartScreen puede mostrar una advertencia. Un análisis antiguo de VirusTotal no acredita esta compilación.

[README](../README.md) · [Estadísticas](ESTADISTICAS-2.0.md) · [Verificación](../VERIFICACION.md)

## Ejecutable preparado

- Archivo: `Contador.exe`, Windows x64, versión de producto `2.0.1`.
- Ruta local de compilación: `dist/2.0.1/Contador.exe` (excluida de Git; adjuntar el archivo a la Release).
- SHA-256: `F16B7BB68ADD03DC50D4B8199E03B5AC248601ABF9D896E78C59FA8A57C0B679`.
- Validación: 302 comprobaciones del motor y 81 de escritorio, 383 en total.

La huella identifica este archivo. Si vuelves a compilarlo o lo firmas, publica la huella del archivo final. La firma digital de editor sigue pendiente.
