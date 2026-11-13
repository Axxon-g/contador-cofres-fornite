<div align="center">
  <img src="docs/images/banner.svg" alt="Contador — Tu botín, en cifras." width="960">
  <h1>Contador de cofres para Fortnite</h1>
  <p>Tu historial de cofres, en una app de Windows.<br>Resumen, partidas y overlay; todo en un solo ejecutable.</p>
  <p>
    <img src="https://img.shields.io/badge/Windows-10%20%2F%2011-1B2B23?style=flat-square" alt="Windows 10 y 11">
    <img src="https://img.shields.io/badge/Arquitectura-x64-1B2B23?style=flat-square" alt="Arquitectura x64">
    <a href="https://github.com/Axxon-g/contador-cofres-fornite/releases"><img src="https://img.shields.io/github/downloads/Axxon-g/contador-cofres-fornite/Contador.exe?style=flat-square&amp;label=Descargas&amp;color=FFC857&amp;labelColor=6E2636&amp;displayAssetName=false" alt="Descargas acumuladas de Contador.exe en todas las versiones" title="Descargas del ejecutable, no personas únicas. Se actualiza automáticamente con caché."></a>
    <img src="https://img.shields.io/badge/Versi%C3%B3n-1.2.6-F1CA76?style=flat-square&amp;labelColor=1B2B23" alt="Versión 1.2.6">
    <a href="LICENSE"><img src="https://img.shields.io/badge/Licencia-MIT-1B2B23?style=flat-square" alt="Licencia MIT"></a>
  </p>
  <p>
    <a href="https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe"><img src="docs/images/download.svg" width="310" alt="Descargar Contador.exe para Windows"></a>
  </p>
  <p><a href="https://github.com/Axxon-g/contador-cofres-fornite/releases">Todas las versiones</a> · <a href="#antes-de-abrir-seguridad-y-aviso-de-windows">Seguridad y SmartScreen</a> · <a href="#primeros-pasos">Primeros pasos</a> · <a href="#preguntas-frecuentes">Preguntas frecuentes</a> · <a href="https://github.com/Axxon-g/contador-cofres-fornite/issues">Informar de un problema</a></p>
  <p><strong>Sin cuentas ni contraseñas. Análisis local en tu equipo.</strong></p>
  <p><sub>La insignia suma las descargas de Contador.exe en todas las versiones publicadas. No cuenta personas únicas y puede tardar en reflejar nuevas descargas.</sub></p>
</div>

> [!IMPORTANT]
> Los cofres se cuentan **al terminar la partida**, cuando Fortnite finaliza la repetición. Debes tener activada la opción **Grabar repeticiones**. La app no muestra ubicaciones de cofres.

<p align="center">
  <img src="docs/images/resumen.png" alt="Pantalla de resumen de Contador: estado de detección, total, sesión y últimas partidas" width="960">
</p>

## Sin cuentas ni contraseñas

**Contador no te pide usuario, correo, contraseña, códigos de verificación ni acceso a tu cuenta de Epic Games.** No tienes que iniciar sesión ni introducir credenciales para utilizarlo.

El análisis se ejecuta localmente, en tu propio equipo, sobre los archivos de repetición que Fortnite ya guarda. El historial y los ajustes permanecen en tu carpeta de datos local. La app no envía estadísticas, no inyecta código ni modifica la memoria del juego. Solo tú decides si exportas o compartes algún archivo.

Utiliza las repeticiones generadas por Fortnite y un lector comunitario de código abierto; **no es una aplicación oficial ni está aprobada por Epic Games**. Leer esos archivos no permite prometer inmunidad a sanciones. Las condiciones aplicables las establece [Epic Games](https://legal.epicgames.com/epicgames/tos).

## Antes de abrir: seguridad y aviso de Windows

### ¿Por qué aparece «Windows protegió su PC»?

<p align="center">
  <img src="docs/images/windows-smartscreen.png" alt="Aviso de Microsoft Defender SmartScreen: aplicación Contador con editor desconocido" width="535">
</p>

El ejecutable distribuido actualmente **no tiene una firma digital de editor verificado**. Microsoft Defender SmartScreen evalúa la reputación del archivo y de su editor; cuando no reconoce la aplicación puede mostrar este aviso y **Editor desconocido**.

**Este aviso, por sí solo, no significa que se haya detectado un virus. Tampoco demuestra que el archivo sea seguro.** Una firma válida identifica al editor, pero incluso una aplicación firmada puede mostrar avisos mientras acumula reputación. Puedes consultar la [explicación oficial de Microsoft](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/smartscreen-reputation) y nuestras [instrucciones de firma digital](docs/FIRMA.md).

No necesitas desactivar Defender ni SmartScreen, añadir exclusiones o instalar certificados del proyecto. Si tu antivirus muestra una detección concreta o no puedes comprobar el origen del archivo, no lo ejecutes y [comparte el resultado en una incidencia](https://github.com/Axxon-g/contador-cofres-fornite/issues).

### Consulta el análisis de VirusTotal

<p align="center">
  <a href="https://www.virustotal.com/gui/file/f0ebdf6c9a65c57b66b97957ab37574e96f32c136201fedcd97be4bb05e2a1c6"><img src="docs/images/virustotal-analysis.png" alt="Captura aportada del análisis de VirusTotal: cero detecciones entre 66 motores en el momento de la captura" width="960"></a>
</p>

La captura aportada muestra **0 detecciones entre 66 motores** en el momento en que se tomó. [Abre el informe en VirusTotal para consultar su estado](https://www.virustotal.com/gui/file/f0ebdf6c9a65c57b66b97957ab37574e96f32c136201fedcd97be4bb05e2a1c6).

Ese informe corresponde exclusivamente al archivo con este **SHA-256**:

```text
f0ebdf6c9a65c57b66b97957ab37574e96f32c136201fedcd97be4bb05e2a1c6
```

La huella del ejecutable local **1.2.5** se comprobó el **12 de noviembre de 2026** y coincide con este informe. Aunque VirusTotal muestre el nombre `Contador.dll`, la coincidencia se comprobó por SHA-256, no por nombre. Esto no verifica automáticamente otras descargas o futuras compilaciones. **Comprueba la huella o analiza tu propio archivo.** Un resultado sin detecciones no garantiza ausencia de malware ni constituye una certificación de seguridad.

### Comprueba tú mismo el archivo descargado

1. Descarga **Contador.exe** únicamente desde las [Releases de este repositorio](https://github.com/Axxon-g/contador-cofres-fornite/releases).
2. Antes de abrirlo, entra en [www.virustotal.com](https://www.virustotal.com/) y selecciona **File / Archivo → Choose file / Elegir archivo**.
3. Selecciona el ejecutable que acabas de descargar. Revisa y acepta las condiciones del servicio antes de enviarlo.
4. Espera el resultado y revisa las detecciones y la fecha del análisis. Haz esta comprobación con cada versión nueva si quieres verificar el archivo que vas a utilizar.

> [!NOTE]
> Subir el ejecutable a VirusTotal es una acción voluntaria que envía ese archivo a un servicio externo. No subas contraseñas, repeticiones, historiales ni otros archivos privados. El análisis local de partidas de Contador no requiere VirusTotal.

Si prefieres comparar el SHA-256 sin subir el archivo, abre PowerShell en su carpeta y ejecuta:

```powershell
Get-FileHash -LiteralPath .\Contador.exe -Algorithm SHA256
```

Solo si coincide exactamente con el SHA-256 anterior estás consultando el informe de ese mismo archivo. Si la Release incluye `Contador.exe.sha256`, también puedes comparar la huella con ese archivo; esto comprueba integridad, pero no sustituye una firma digital ni un análisis de seguridad.

## Primeros pasos

1. **Descarga [Contador.exe](https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe)** y guárdalo en la carpeta que prefieras.
2. **Activa las repeticiones en Fortnite:** Ajustes → Juego → Repeticiones → **Grabar repeticiones**.
3. **Abre Contador.exe con doble clic.** El historial anterior compatible se recupera automáticamente si está en este equipo.
4. **Deja la app abierta mientras juegas.** Al terminar una partida de Battle Royale, analizará la repetición y sumará tus cofres.

Solo necesitas el ejecutable. No hace falta descargar el código, abrir VS Code ni ejecutar archivos de compilación.

Si la carpeta no se encuentra, entra en **Ajustes → Elegir carpeta**. La ubicación habitual es:

```text
%LOCALAPPDATA%\FortniteGame\Saved\Demos
```

## Todo tu botín, en un mismo sitio

| En la app | Para qué sirve |
|---|---|
| **Resumen** | Consulta el total de tu cuenta, la sesión, la última partida, la media y tu récord. |
| **Historial** | Busca por fecha o modo, ordena columnas y selecciona una partida para ver sus detalles. |
| **Exportar CSV** | Guarda las partidas del perfil y filtro visibles para abrirlas en una hoja de cálculo. |
| **Perfiles** | Cambia entre las cuentas detectadas; sus estadísticas se mantienen separadas. |
| **Overlay** | Muestra las cifras sobre el juego y ajusta su tamaño desde la app. |
| **Pausar detección** | Detiene los siguientes análisis. El análisis que ya empezó termina normalmente. |
| **Datos locales** | Abre el informe, la carpeta de datos y el registro de errores desde Ajustes. |

<table>
  <tr>
    <td width="50%"><img src="docs/images/historial.png" alt="Historial con buscador, columnas de cofres y datos de demostración"><br><strong>Cada partida, en detalle.</strong></td>
    <td width="50%"><img src="docs/images/ajustes.png" alt="Ajustes de carpeta, cuenta y overlay"><br><strong>El contador, a tu manera.</strong></td>
  </tr>
</table>

<sub>Vistas previas renderizadas desde la interfaz de la app. La tabla de historial utiliza datos de demostración; no se incluyen en el programa.</sub>

## Elegir una cuenta

Entra en **Ajustes → Cuenta** y selecciona el perfil. Cada opción muestra su nombre, partidas y cofres. Desde la versión **1.2.6**, el selector permanece disponible mientras se analizan repeticiones; si hay un análisis en curso, la elección se aplica al terminar. El resumen, historial, informe y overlays utilizan la cuenta elegida, que se recuerda al volver a abrir la app.

Solo aparecen cuentas con partidas detectadas o importadas en este equipo. No se consulta el historial de otras cuentas por Internet ni se necesitan sus credenciales.

## ¿Ya usabas otro contador?

**Abre la app y deja que recupere tu historial.** Desde la versión 1.1, Contador busca automáticamente los datos locales de **ChestCounter** y **ChestTracker**:

- Incorpora las partidas que faltan sin duplicar las que ya están guardadas.
- Si una partida tiene recuentos de distintas versiones, conserva el registro del analizador más reciente.
- Crea una copia de seguridad antes de guardar la incorporación.
- Mantiene los perfiles separados por cuenta.
- Lee el historial anterior sin modificar los archivos de la otra aplicación.

Los historiales se buscan en `%APPDATA%\ChestCounter\data.json` y `%APPDATA%\ChestTracker\data.json`. Los respaldos se guardan en `%APPDATA%\ContadorModern\Backups`.

La recuperación es local a cada equipo. No necesitas scripts ni copiar archivos. Si un historial está dañado o es incompatible, la app lo indica y vuelve a intentarlo al siguiente inicio. **Esta función incorpora datos a Contador; no sincroniza las aplicaciones entre sí.**

## Aviso de partida y contadores claros

La app comprueba la cabecera de las repeticiones cada dos segundos para detectar que Fortnite está grabando. Muestra **EN PARTIDA** en la ventana y en el overlay, incluso si la detección de cofres está pausada. Al dejar de grabarse, retira el aviso. La detección depende de que Fortnite haya creado y actualizado la repetición; no consulta la memoria del juego.

- **Última partida:** cofres de la partida finalizada más reciente.
- **Esta sesión:** cofres de las partidas jugadas desde que abriste la app. Se reinicia al abrirla y excluye el historial importado y las repeticiones antiguas reanalizadas.
- **Tesoros:** total de tesoros encontrados en el perfil seleccionado, con su etiqueta visible.

Última partida y sesión pueden coincidir si solo has terminado una partida durante esta ejecución. Los cofres siguen contabilizándose al finalizar; el aviso de partida no es un recuento instantáneo de cada apertura.

<p align="center"><img src="docs/images/overlay-en-partida.png" alt="Overlay de demostración: en partida, última partida 17, sesión 83, tesoros 22" width="260"></p>

La versión 1.2 incorpora una paleta azul marino, rojo y dorado de inspiración pirata y la moneda aportada como referencia visual, integrada en el ejecutable.

## Overlay para jugar y transmitir

Actívalo en **Ajustes → Mostrar contador sobre el juego**. En Fortnite, selecciona **Pantalla completa en ventana** para que Windows pueda mostrarlo encima.

| Atajo | Acción |
|---|---|
| <kbd>Ctrl</kbd> + <kbd>Mayús</kbd> + <kbd>O</kbd> | Mostrar u ocultar el overlay. |
| <kbd>Ctrl</kbd> + <kbd>Mayús</kbd> + <kbd>L</kbd> | Bloquear o desbloquear el overlay. |

Desbloquéalo para arrastrarlo a otra posición. Bloquéalo al jugar para que el ratón lo atraviese.

**Para OBS:** añade una fuente **Navegador**, activa **Archivo local** y selecciona `overlay.html` en la carpeta de datos. Puedes encontrarla desde **Ajustes → Abrir datos**. Desde la versión 1.2.3, la fuente de OBS usa el mismo HUD que aparece sobre el juego: moneda, fondo transparente, estado, última partida, sesión y tesoros. Se actualiza cada dos segundos sin recargar toda la página. Mantén la app abierta y conserva `overlay-data.js` junto a `overlay.html`; ambos se generan automáticamente. Si tenías la fuente anterior, actualízala una vez en OBS después de abrir esta versión. Usa inicialmente un ancho de 300 y alto de 180 píxeles, y ajusta la escala en la escena.

## Cómo funciona

Contador lee los archivos `.replay` que Fortnite guarda en tu equipo. Identifica al jugador que grabó la partida y los contenedores que abrió, con el lector de [LED-N/Fortnite-ChestCounter](https://github.com/LED-N/Fortnite-ChestCounter).

- Solo lee las repeticiones; no modifica el juego.
- No accede a la memoria de Fortnite ni a tus credenciales de Epic.
- Las cajas de munición y otros contenedores se distinguen de los cofres.
- Una partida renombrada no vuelve a sumarse.
- Los historiales y preferencias se guardan localmente; la app no envía estadísticas.

Desde la versión 1.2.5 se revisan las repeticiones cada segundo. Tras finalizar, se comprueba brevemente su estabilidad y se muestran primero las cifras; el informe se genera después. La duración real depende del tamaño del archivo y del equipo.

Fortnite escribe la clave necesaria para leer la repetición al finalizar la partida. Por eso el total se actualiza después, aunque la app esté abierta durante el juego.

### Diagrama del proyecto

<p align="center">
  <a href="https://gitdiagram.com/axxon-g/contador-cofres-fornite"><img src="docs/images/architecture-diagram.png" alt="Diagrama del proyecto: lectura de repeticiones, análisis, historial local, interfaz y overlays" width="960"></a>
</p>

[Explora el diagrama interactivo en GitDiagram](https://gitdiagram.com/axxon-g/contador-cofres-fornite) para ver los componentes y sus relaciones. En términos sencillos: **Fortnite guarda la repetición → Contador la lee y analiza → guarda el historial local → actualiza la ventana, el overlay y el informe**.

La imagen es una captura orientativa del diagrama generado por una herramienta externa; puede quedar desactualizada y no constituye una auditoría de seguridad. Puedes contrastarla con el código de este repositorio.

## Preguntas frecuentes

<details>
<summary><strong>No aparece ningún cofre. ¿Qué reviso?</strong></summary>

Comprueba que **Grabar repeticiones** está activado y que jugaste una partida de Battle Royale. Verifica la carpeta en Ajustes y deja terminar la partida. Si aparece un error, abre **Ajustes → Registro de errores**.

Fortnite puede crear la carpeta de repeticiones después de la primera partida grabada.

</details>

<details>
<summary><strong>El total es distinto al de mi contador anterior.</strong></summary>

Comprueba que estás viendo la misma cuenta y que la recuperación del historial terminó. Los totales también pueden diferir si las aplicaciones usan versiones distintas del analizador. Contador conserva el registro del analizador más reciente para una partida compartida; no suma ambos registros.

La importación recupera partidas conocidas por el contador anterior. No puede reconstruir partidas que ninguna aplicación guardó y cuya repetición ya no está disponible.

</details>

<details>
<summary><strong>¿Cuenta en tiempo real, mientras abro cada cofre?</strong></summary>

El recuento final se obtiene **después de la partida**, cuando Fortnite termina de escribir la repetición. Durante la grabación, la app puede indicar que hay una partida en curso.

</details>

<details>
<summary><strong>¿Qué pasa si Fortnite se cierra o cambia su formato?</strong></summary>

Una repetición incompleta o incompatible puede no contarse. Una actualización de Fortnite puede requerir actualizar el lector. El historial ya guardado se conserva.

Si necesitas ayuda, [abre una incidencia](https://github.com/Axxon-g/contador-cofres-fornite/issues) con la versión de la app y el mensaje de error. Revisa el registro antes de compartirlo: puede contener datos de tus partidas.

</details>

<details>
<summary><strong>¿Tengo que instalar .NET o descargar “Source code”?</strong></summary>

No. Para usar la app, descarga **Contador.exe** en la sección **Assets** de una Release. Los archivos **Source code (zip)** y **Source code (tar.gz)** contienen el código para desarrolladores.

</details>

<details>
<summary><strong>¿Cómo actualizo la app sin perder mis partidas?</strong></summary>

Cierra Contador, descarga el nuevo ejecutable desde [Releases](https://github.com/Axxon-g/contador-cofres-fornite/releases) y reemplaza el ejecutable anterior. El historial se guarda en una carpeta independiente, no dentro del archivo .exe.

</details>

## Tus datos

Desde **Ajustes → Abrir datos** puedes acceder a `%APPDATA%\ContadorModern`.

| Archivo o carpeta | Contenido |
|---|---|
| `data.json` | Historial de partidas. |
| `config.json` | Preferencias y cuenta seleccionada. |
| `Backups/` | Respaldos de la importación automática. |
| `report.html` | Informe con los colores, moneda y tipografía del launcher. Se regenera automáticamente. |
| `overlay.html` | HUD transparente para la fuente Navegador de OBS. |
| `overlay-data.js` | Cifras y estado actualizados automáticamente por la app. |
| `matches.csv` | Exportación generada del historial. |
| `errors.log` | Diagnóstico, cuando se registran errores. |

Para hacer un respaldo o cambiar de equipo, copia la carpeta de datos con Contador cerrado.

<details>
<summary><strong>Para desarrolladores: compilar y comprobar</strong></summary>

Instala el [SDK de .NET 10 para Windows x64](https://dotnet.microsoft.com/download/dotnet/10.0) y ejecuta desde la raíz del proyecto:

```powershell
.\build.bat
```

El ejecutable autónomo se genera en `dist\1.2.6\Contador.exe`. La primera compilación necesita conexión para restaurar dependencias.

Para desarrollar o ejecutar las comprobaciones:

```powershell
dotnet run --project src/Contador/Contador.csproj
dotnet run --project tests/Contador.Checks/Contador.Checks.csproj -c Release
dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release
```

La versión 1.2.5 pasó 63 comprobaciones del motor y 3 pruebas de cierre WPF, incluidas pruebas de importación sobre una copia de historial real. Esto comprueba conservación y combinación de datos; no sustituye validar el lector contra nuevas versiones de Fortnite.

Consulta [cómo publicar el ejecutable en GitHub Releases](docs/PUBLICAR.md) y el [estado de verificación](VERIFICACION.md).

</details>

## Créditos y licencia

[MIT](LICENSE). La interfaz de escritorio utiliza el motor de [LED-N/Fortnite-ChestCounter](https://github.com/LED-N/Fortnite-ChestCounter), basado en [FortniteReplayDecompressor de Shiqan](https://github.com/Shiqan/FortniteReplayDecompressor) y OozSharp. Se conservan sus licencias y notas en [vendor/ChestCounter](vendor/ChestCounter) y [vendor/UPSTREAM.md](vendor/UPSTREAM.md).

Este proyecto no está afiliado, aprobado ni patrocinado por Epic Games. Fortnite es una marca de Epic Games.

<div align="center">
  <br>
  <a href="https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe"><strong>Descargar Contador para Windows</strong></a>
  <p>Abre la app. Juega tu partida. Consulta tu botín.</p>
</div>
