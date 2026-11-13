# Publicar Contador 1.3 en GitHub

El repositorio contiene el código; GitHub Releases distribuye el programa. Los usuarios solo necesitan **Contador.exe**, sin VS Code, SDK ni scripts.

## Preparar la compilación

1. Conserva `src`, `tests`, `vendor`, `global.json` y `NuGet.Config`. No elimines `vendor`: el lector se compila desde esa carpeta.
2. Instala el SDK de .NET 10 para Windows x64.
3. La versión del proyecto es `1.3.0` y la interfaz deriva su etiqueta v1.3 de esa versión. Desde PowerShell en la raíz, ejecuta ` .\build.bat `.
4. Comprueba que las pruebas y la publicación terminan correctamente. No publiques una compilación si fallan las comprobaciones.
5. El resultado queda en `dist\1.3.0\Contador.exe`, acompañado de `Contador.exe.sha256`. Cierra el contador antes de recompilar sobre ese archivo.

El mapa y su catálogo están integrados. No adjuntes recursos sueltos, archivos PDB, carpetas de AppData, repeticiones ni historiales de prueba.

## Subir el código

Revisa los cambios antes de confirmarlos. Incluye los archivos nuevos de código, el catálogo del mapa, su recurso de imagen, las pruebas, README y `docs/images`. `dist/` se excluye del código: el ejecutable se adjunta en Releases.

## Publicar o actualizar la Release

1. Abre [Releases](https://github.com/Axxon-g/contador-cofres-fornite/releases).
2. Si **v1.3.0 ya existe**, edítala. Si aún no existe, crea una Release con tag **v1.3.0** y la rama que contiene el código actualizado. No reutilices ni muevas un tag existente hacia otro commit sin revisar sus consecuencias.
3. Título: **Contador 1.3 — Mapa HD, rutas e informe**.
4. Copia las [notas de esta versión](RELEASE-v1.3.0.md) en la descripción.
5. Adjunta **dist/1.3.0/Contador.exe** y **Contador.exe.sha256**. Si editas una Release, reemplaza los assets anteriores con esos nombres por los recién compilados.
6. Espera a que termine la carga. Usa **Latest** para esta versión estable y publica o guarda los cambios.
7. Prueba el [enlace del README](https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe). Descarga el archivo y compara su SHA-256 con el publicado.

Los archivos Source code los genera GitHub automáticamente: no son la aplicación lista para abrir. El nombre **Contador.exe** debe mantenerse para que funcionen el botón y la insignia del README.

## Seguridad y actualización

Cada compilación puede tener otra huella. El informe de VirusTotal de 1.2.5 no acredita el ejecutable nuevo. Si añades un análisis nuevo, enlaza el informe correspondiente a su SHA-256 exacto. No afirmes ausencia de virus ni aprobación de Epic.

El ejecutable no tiene editor verificado salvo que lo firmes con un certificado válido. Consulta [FIRMA.md](FIRMA.md). Para actualizar, el usuario cierra la app y sustituye el ejecutable; el historial y el plan del mapa se conservan en su carpeta de datos independiente.

[Referencia oficial de GitHub](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository).
