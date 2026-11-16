# Contador 2.2 — Interfaz por bloques, tesoros y recuperación de lectura

## Novedades

- Resumen, Mapa y rutas, Tesoros y Mi progreso organizados en bloques, con la paleta roja, azul y dorada.
- Botón visible para activar el conteo visual opcional, con permiso por ejecución y sin guardar ni enviar capturas.
- Tesoros y probabilidades: tabla comparativa y simulación de cantidades y Horas del tesoro independiente del historial.
- Mi progreso: totales, gráfica diaria, tipos de cofre y origen de recompensas, conservando el informe HTML.
- Perfil público: estadísticas por nombre y métricas de eliminaciones por partida y tiempo jugado cuando el proveedor las ofrece. No requiere claves del usuario.
- Búsqueda de actualizaciones al iniciar o reabrir la ventana. El botón cambia entre Buscar actualizaciones y Nueva actualización; la descarga e instalación siguen siendo manuales.
- Reintentos limitados para fallos temporales, reintento manual de archivos fallidos, copias locales opcionales y diagnóstico sin nombres, ID, rutas ni claves.

## Límites

El historial depende de repeticiones locales finalizadas y compatibles. El conteo
visual y el grupal siguen siendo experimentales y pueden ser incompletos. No se
suma la estimación visual al historial. Las tasas de premios son una referencia
comunitaria pendiente de verificación con Epic; no predicen ni garantizan premios.
No se incorpora un servicio de amigos ni una API de cofres por nombre.

Las protecciones de exportación CSV y los datos locales se conservan. Esta revisión
no acredita que todos los formatos recientes de Fortnite sean compatibles; para
investigar un archivo fallido necesitamos una muestra afectada o su diagnóstico.

## Actualizar

1. Cierra Contador desde la bandeja junto al reloj → Salir.
2. Sustituye el ejecutable por Contador.exe de esta Release.
3. Abre la app y revisa cuenta, carpeta y período en Ajustes.

No borres `%APPDATA%\ContadorModern`: contiene tu historial y preferencias.
Windows puede mostrar SmartScreen por falta de firma digital de editor verificado.
Puedes analizar tu descarga en VirusTotal; el análisis antiguo no acredita este archivo.

## Archivos para la Release

- Etiqueta y título: **v2.2** · **Contador 2.2**.
- Adjuntar **Contador.exe** de la carpeta local `dist/2.2.0/` (ignorada por Git).
- SHA-256: `868DBD634EB4CDCED535AE0D177829A63A72F5F60E0DB36DF90D54D26FC4CAD5`.
- Compilación autocontenida Windows x64: 170.803.490 bytes; no requiere instalar .NET.

La versión pública es 2.2. Los campos técnicos de ensamblado usan la representación
normalizada 2.2.0 / 2.2.0.0. Si se recompila o firma el archivo, su huella cambia:
publica la del ejecutable final que adjuntes.

## Verificación

365 comprobaciones del motor y 93 de escritorio aprobadas, 458 en total. Compilación
Release y revisión de recursos y enlaces locales completadas. La búsqueda de patrones
de credenciales no encontró coincidencias en los cambios publicables; no constituye
una garantía ni una auditoría completa de dependencias. No se incluyen configuraciones
locales, certificados privados, replays, registros ni archivos duplicados de sincronización.

[README](../README.md) · [Verificación](../VERIFICACION.md) · [Probabilidades](PROBABILIDADES.md)
