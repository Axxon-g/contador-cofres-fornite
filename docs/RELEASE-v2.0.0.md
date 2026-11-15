# Contador 2.0.0 — Nuevo launcher, equipo y conteo opcional

## Novedades

- Launcher renovado con rojo pirata, azul mar y dorado; moneda integrada y cofre antes del total personal.
- Cuenta accesible en la cabecera y seis apartados: Resumen, Historial, Equipo, Mapa y rutas, Informe y Ajustes.
- Equipo por partida y jugador, con procedencia y cobertura visibles.
- Importación local de `.replay` finalizados de compañeros: misma sesión verificada y pertenencia al equipo conocida. Conserva la mejor cifra por jugador sin sumar perspectivas duplicadas.
- Total combinado y filas individuales utilizan la misma conciliación. Puedes retirar una fuente sin borrar archivos ni historial personal; un archivo no apto no cancela otros válidos del lote.
- Conteo en tiempo real opcional mediante avisos XP/PE del HUD, con permiso por ejecución y desactivación inmediata.
- Registro común de interacciones: deduplicación, rellenos, estado conservado entre actualizaciones parciales y autor identificado con retraso dentro del mismo ciclo.
- Se mantienen las protecciones del lector, CSV frente a fórmulas, publicación estable de overlays, mapa HD opcional y segundo plano.

## Qué sigue siendo experimental

**Equipo:** `≥` significa mínimo registrado. Una o varias repeticiones pueden omitir acciones; no es un total exacto del servidor. Importar perspectivas no cambia tus cofres ni tus recompensas personales. El origen estimado de recompensas sigue identificado como tal.

**Tiempo real:** es una estimación separada del historial. Los cofres rotos sin aviso XP/PE, cambios de escala, HUD o errores de OCR pueden producir diferencias. Los compañeros de la última partida se muestran como datos del replay finalizado; no son cifras en vivo de todo el equipo.

El permiso de captura no se conserva al reiniciar. Solo se procesan regiones del HUD de Fortnite en primer plano, en memoria; no se guardan ni envían imágenes. Rechazar el permiso conserva el análisis por repeticiones.

## Actualizar

1. Sal de Contador desde la bandeja de Windows → **Salir**.
2. Sustituye el ejecutable anterior por **Contador.exe** de esta versión.
3. Abre la app y revisa cuenta y período.

No borres `%APPDATA%\ContadorModern`: el historial se conserva allí. La migración guarda un respaldo previo. Solo pueden volver a analizarse repeticiones que todavía existan.

## Ampliar el equipo

Espera a que termine tu análisis → **Equipo** → elige partida → **Importar repeticiones del equipo**. Selecciona archivos finalizados de tus compañeros de esa misma partida. Consulta Origen y Cobertura; solo se guarda un resumen local. No se piden credenciales ni se suben archivos.

## Verificación y seguridad

Se verifican reglas del motor y controles de escritorio con datos ficticios. Los recuentos y límites están documentados en [VERIFICACION.md](../VERIFICACION.md). Todavía falta contrastar varias perspectivas contra un conteo manual de una misma partida; las pruebas no acreditan exactitud universal.

El ejecutable no tiene firma de editor verificado: Windows puede mostrar SmartScreen. Un análisis antiguo de VirusTotal no acredita este archivo; comprueba tu descarga concreta. No se necesitan contraseñas de Epic ni desactivar el antivirus.

[Investigación](INVESTIGACION-v2.0.md) · [Conteo visual](LECTURA-VISUAL-2.0.md) · [README](../README.md)

## Archivo preparado para la Release

- Nombre: `Contador.exe`
- Versión: 2.0.0 (Windows x64)
- SHA-256: `32B58B57EBB02B2B42C694954D1D49EF622DC4DB0AFFB209AA6951D53B41C076`

La huella corresponde al ejecutable preparado con esta revisión. Si vuelves a compilar o firmas el archivo, calcula y publica la huella del archivo final.
