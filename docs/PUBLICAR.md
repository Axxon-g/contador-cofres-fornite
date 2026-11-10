# Publicar Contador.exe en GitHub

El código del repositorio y el programa descargable se publican por separado. Usa **GitHub Releases** para distribuir el ejecutable. Los usuarios solo necesitan descargar Contador.exe.

## Antes de publicar

Sube los cambios del proyecto, incluidos README.md y la carpeta docs/ con sus imágenes. La versión 1.1 necesita también el código de importación y sus pruebas: no publiques solo el README si todavía no subiste esos cambios.

El ejecutable ya compilado está en:

```text
C:\Users\axxob\OneDrive\Documentos\GitHub\contador\dist\1.2.1\Contador.exe
```

Si cambias el código, vuelve a compilar antes de adjuntar el archivo. Mantén el nombre **Contador.exe**: el enlace de descarga del README lo utiliza.

## Crear la primera Release

1. Abre [la página de nueva Release](https://github.com/Axxon-g/contador-cofres-fornite/releases/new).
2. En **Choose a tag**, escribe **v1.2.1** y elige **Create new tag** si todavía no existe.
3. En **Target**, selecciona la rama que contiene el código actualizado.
4. Usa como título **Contador 1.2.1 — Identidad pirata**.
5. Copia el contenido de [RELEASE-v1.2.1.md](RELEASE-v1.2.1.md) en la descripción.
6. En el área para adjuntar binarios, arrastra **dist/1.2.0/Contador.exe**. Espera a que termine la carga.
7. Comprueba que aparece el archivo **Contador.exe**, selecciona **Set as latest release** y pulsa **Publish release**.

Una vez publicada, verifica el [enlace directo de descarga](https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe). Ese es el enlace utilizado por el botón del README. Antes de publicar la Release y adjuntar el ejecutable, el enlace puede mostrar un error 404.

GitHub añadirá también dos archivos llamados Source code. Los genera automáticamente y contienen el código, no la aplicación lista para usar.

## Próximas versiones

Actualiza la versión del proyecto y las notas, compila, crea un tag nuevo (por ejemplo v1.1.1) y adjunta el ejecutable actualizado con el mismo nombre. Marca la versión estable como latest: el botón del README seguirá apuntando al archivo de la versión más reciente.

No necesitas incluir historiales personales, archivos de AppData ni scripts de importación. Cada equipo genera sus propios datos; la importación ya está integrada en la app.

## Referencia

[Documentación oficial: administrar Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository).
