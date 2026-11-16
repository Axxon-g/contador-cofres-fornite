<div align="center">
  <img src="docs/images/banner.svg" width="960" alt="Contador — Tu botín, en cifras. Una aplicación de escritorio para Fortnite.">
  <h1>Contador de cofres para Fortnite · 2.0.1</h1>
  <p><strong>Tu botín, en cifras.</strong><br>Historial personal, equipo, mapa, informe y overlay para Windows.</p>
  <p>
    <img src="https://img.shields.io/badge/Windows-10%20%2F%2011-183D60?style=flat-square" alt="Windows 10 y 11">
    <img src="https://img.shields.io/badge/x64-2.0.1-FFD05A?style=flat-square&amp;labelColor=80273C" alt="Versión 2.0.1 para x64">
    <a href="https://github.com/Axxon-g/contador-cofres-fornite/releases"><img src="https://img.shields.io/github/downloads/Axxon-g/contador-cofres-fornite/Contador.exe?style=flat-square&amp;label=Descargas&amp;color=FFD05A&amp;labelColor=80273C&amp;displayAssetName=false" alt="Descargas acumuladas del ejecutable"></a>
    <a href="LICENSE"><img src="https://img.shields.io/badge/Licencia-MIT-183D60?style=flat-square" alt="Licencia MIT"></a>
  </p>
  <p>
    <a href="https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe"><img src="docs/images/download.svg" width="310" alt="Descargar Contador.exe para Windows x64"></a>
  </p>
  <p><a href="https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe">Descarga directa de Contador.exe</a> · <a href="https://github.com/Axxon-g/contador-cofres-fornite/releases">Todas las versiones y notas de actualización</a></p>
  <p><a href="#primeros-pasos">Primeros pasos</a> · <a href="#equipo--experimental">Equipo</a> · <a href="#conteo-en-tiempo-real--experimental">Tiempo real</a> · <a href="#seguridad-y-privacidad">Seguridad</a> · <a href="#compilar-desde-el-código">Compilar</a></p>
  <p><strong>Sin iniciar sesión ni introducir contraseñas de Epic.</strong></p>
</div>

> [!IMPORTANT]
> El historial se calcula con **repeticiones locales finalizadas**. Activa **Grabar repeticiones** en Fortnite. El conteo visual en tiempo real es opcional y experimental; no sustituye al historial ni garantiza detectar cada apertura.

La descarga apunta a la última versión **publicada** en GitHub, que puede ser anterior a 2.0.1 hasta que se publique su Release. La insignia cuenta descargas del archivo, no personas únicas, y puede actualizarse con retraso.

## Lo nuevo en 2.0.1

- **Estadísticas públicas:** busca un jugador por nombre Epic, PlayStation o Xbox desde el nuevo apartado bajo Resumen. No necesitas clave de API ni contraseña.
- **Conexión corregida:** la app consulta directamente el dominio definitivo del servicio de AXXON, evitando la redirección que impedía cargar los resultados.
- **Privacidad documentada:** solo se consulta el nombre o ID y los filtros elegidos. La clave del proveedor permanece en el servidor; replays, capturas e historial de cofres no se envían.
- Se mantienen las funciones de 2.0 y el historial local. [Notas de actualización](docs/RELEASE-v2.0.1.md).

## Lo nuevo en 2.0

| Función | Qué cambia |
|---|---|
| **Launcher renovado** | Rojo pirata, azul mar y dorado; moneda integrada y cofre junto al total personal. |
| **Equipo por jugador** | Una sección propia para elegir partida, consultar cada cifra y conocer su origen y cobertura. |
| **Fuentes complementarias** | Importa repeticiones de compañeros de la misma partida para ampliar la cobertura sin duplicar aportaciones. |
| **Conteo visual opcional** | Estimación durante la partida a partir de avisos XP/PE, con permiso por ejecución. |
| **Registro de aperturas** | Política común para cofres propios y del equipo, con tratamiento de duplicados, rellenos y autor identificado con retraso. |
| **Cuenta siempre accesible** | Selección de perfil en la cabecera; resumen, historial, equipo e informe comparten el contexto. |

Se mantienen el mapa con terreno HD opcional, las rutas editables, filtros de fechas, exportación CSV, informe nativo, overlays y ejecución en segundo plano. Las protecciones de lectura y la protección de CSV frente a fórmulas se conservan.

<p align="center">
  <img src="docs/images/resumen-2.0.png" width="1000" alt="Contador 2.0: resumen, cofre junto al total personal, selección de cuenta y últimas partidas">
  <br><sub>Captura de uso aportada por el autor. Las cifras son de su historial, no datos incluidos en la descarga.</sub>
</p>

## Primeros pasos

1. Pulsa **Descargar Contador.exe** en el botón de arriba o utiliza la [descarga directa](https://github.com/Axxon-g/contador-cofres-fornite/releases/latest/download/Contador.exe). También puedes elegir una versión desde [GitHub Releases](https://github.com/Axxon-g/contador-cofres-fornite/releases). No necesitas descargar el código fuente.
2. Activa **Grabar repeticiones** en Fortnite y juega una partida compatible con el lector.
3. Abre Contador. En **Ajustes**, comprueba la carpeta de repeticiones; puedes elegirla si no se detecta automáticamente.
4. Selecciona tu **Cuenta** en la cabecera. Solo aparecen perfiles encontrados en el historial local.
5. Al terminar la partida, espera a que Fortnite finalice el archivo. La app analiza la repetición y actualiza las cifras.

**Requisitos:** Windows x64, repeticiones guardadas en ese equipo y espacio para ellas. El ejecutable incluye .NET; no necesitas instalar el SDK para usarlo. El OCR opcional requiere Windows 10 versión 2004 o posterior, un idioma de reconocimiento instalado y un HUD compatible.

**Al actualizar:** sal desde la bandeja de Windows → **Salir**, sustituye `Contador.exe` y abre la versión nueva. No borres `%APPDATA%\ContadorModern`: allí están el historial y las preferencias. La migración conserva un respaldo previo; solo puede volver a analizar repeticiones que todavía existan.

## Estadísticas públicas sin claves de usuario

El apartado **Estadísticas**, debajo de Resumen, permite buscar por nombre Epic, PlayStation o Xbox, o utilizar el perfil local. La consulta pasa por el servicio de AXXON; los usuarios no configuran claves y el ejecutable no incluye la clave del proveedor. [Configuración del servicio y límites](docs/ESTADISTICAS-2.0.md).

El servicio está disponible en [AXXON](https://www.axxon.dev/fortnite-tracker/). El nombre o ID público y los filtros se envían a AXXON y Fortnite-API; el alojamiento puede registrar datos técnicos de la consulta, incluida la dirección IP. Los resultados pueden proceder de una caché de hasta cinco minutos. Las cuentas privadas o los límites del proveedor pueden impedir una consulta. No se muestran datos ficticios como resultado real.

## Equipo · experimental

Tu repetición puede omitir aperturas lejanas de compañeros. **≥ significa mínimo registrado, no total exacto del servidor.** Incluso varias perspectivas pueden seguir siendo incompletas.

1. Deja que termine el análisis de tu partida.
2. Abre **Equipo** y selecciona la partida.
3. Pulsa **Importar repeticiones del equipo** y elige archivos `.replay` finalizados que te hayan compartido tus compañeros de esa misma partida.
4. Revisa las columnas **Jugador**, **Cofres**, **Origen** y **Cobertura**.

La app verifica sesión e identidad y que el grabador pertenezca al equipo conocido; no vincula fuentes solo por nombre. Conserva la mejor cifra disponible por jugador, sin sumar varias veces una misma contribución. Una observación desde otro compañero sigue siendo parcial.

<p align="center">
  <img src="docs/images/equipo-fuentes-2.0.png" width="1000" alt="Equipo: partida seleccionada, mínimo registrado, desglose por jugador y origen de las cifras">
  <br><sub>Vista de prueba con datos ficticios para explicar las fuentes; no se incluyen en el historial del usuario.</sub>
</p>

- Los archivos se analizan localmente; no se suben ni se copian a un servidor.
- Puedes retirar una fuente individual o todas las fuentes importadas sin borrar sus archivos ni tu historial personal.
- Un archivo no apto no impide aprovechar otros válidos del lote. Se admiten hasta 20 archivos por operación y 1.000 resúmenes.
- Las importaciones del equipo no cambian tus cofres ni tus recompensas personales.
- El resumen acumulado distingue tus cofres y los compañeros detectados. La cobertura muestra las partidas que carecen de datos.

Consulta la [investigación y sus límites](docs/INVESTIGACION-v2.0.md). Todavía falta validar varias perspectivas de una misma partida frente a un conteo manual completo; no se anuncia precisión perfecta.

## Conteo en tiempo real · experimental

En **Resumen**, pulsa **Activar conteo en tiempo real**. La app explica la captura y pide permiso; rechazar mantiene el análisis de repeticiones. Puedes desactivarlo en cualquier momento. El permiso no se guarda: al volver a abrir empieza desactivado.

<p align="center">
  <img src="docs/images/conteo-tiempo-real-2.0.png" width="1000" alt="Activación opcional del conteo en tiempo real y mínimos por jugador de la última partida finalizada">
  <br><sub>Captura aportada por el autor. El equipo mostrado corresponde a la última partida finalizada, no a cifras en vivo de los compañeros.</sub>
</p>

| Cifra | Cómo interpretarla |
|---|---|
| **Total personal** | Historial de repeticiones del perfil y período seleccionados. |
| **≈ estimación visual** | Avisos de cofres con XP/PE reconocidos en pantalla; puede omitir o duplicar detecciones. |
| **Equipo de la última partida** | Cifras del replay finalizado y fuentes vinculadas; no un conteo en directo de todos los compañeros. |

Solo se procesan regiones del HUD de Fortnite en primer plano, en memoria. No se guardan ni envían capturas. Se pausa al perder el foco, detener la detección o dejar de detectarse una partida.

Los cofres rotos sin aviso XP/PE, HUD personalizado, cambios de escala y errores de reconocimiento pueden producir diferencias. El resultado final de la repetición se calcula por separado: **no se suma la estimación visual al historial**. [Funcionamiento, permiso y limitaciones](docs/LECTURA-VISUAL-2.0.md).

## Mapa, rutas e informe

**Mapa y rutas** permite acercar, desplazar, buscar, activar capas, seleccionar puntos y crear recorridos manuales. La base integrada funciona sin conexión. **Terreno HD · Internet** está activado por defecto y utiliza piezas de Fortnite.GG con caché local y respaldo integrado; puedes desactivarlo.

El catálogo integrado corresponde a Treasure Hunters **43.10**: 2.146 referencias comunitarias de botín y 16 lugares. Es una instantánea fija, **no se actualiza con cada temporada**, no detecta el botín durante la partida y no garantiza premios. Las bóvedas son referencias orientativas. Las rutas unen paradas manuales; no calculan un camino transitable.

**Informe** reúne totales, sesión, media, récord, tipos de cofre, tesoros y partidas sin salir del programa. Conserva **Abrir versión HTML**. Los filtros de **Ajustes → Fechas del contador** se aplican al resumen, historial, equipo, informe y overlays sin borrar partidas.

- [Controles y procedencia del mapa](docs/MAPA-INTERACTIVO-1.2.7.md)
- [Terreno HD, privacidad y proveedor externo](docs/MAPA-HD-1.2.7.md)
- [Rutas y fuentes comunitarias](docs/RUTAS-1.2.7.md)

## Overlay y segundo plano

Activa el overlay desde **Ajustes** y utiliza Fortnite en pantalla completa en ventana para permitir que Windows lo muestre encima.

| Atajo | Acción |
|---|---|
| **Ctrl + Mayús + O** | Mostrar u ocultar el overlay. |
| **Ctrl + Mayús + L** | Bloquear o desbloquear su posición. |

Para OBS, añade una fuente **Navegador → Archivo local** y selecciona `overlay.html` desde la carpeta de datos. Conserva `overlay-data.js` a su lado; ambos se generan automáticamente. Mantén Contador abierto.

Al cerrar la ventana puedes **Mantener en segundo plano**, **Salir** o **Cancelar**. La moneda junto al reloj permite abrir Contador o salir con el menú contextual. Si no aparece, revisa **Mostrar iconos ocultos** de Windows.

## Seguridad y privacidad

**No se piden contraseñas, correo, códigos de verificación ni acceso a tu cuenta de Epic.** Los perfiles se identifican a partir de repeticiones locales. El historial y los ajustes se guardan en `%APPDATA%\ContadorModern`.

El análisis no modifica el juego ni lee su memoria. El conteo visual requiere permiso y no guarda ni envía capturas. Consultar **Estadísticas** envía únicamente el nombre o ID público y los filtros al servicio de AXXON y a Fortnite-API; no transmite replays, capturas ni historial local. La opción de usar la cuenta seleccionada puede actualizar esa consulta al cambiar de perfil después de buscar. El modo HD realiza conexiones a Fortnite.GG para obtener imágenes, y abrir enlaces utiliza tu navegador.

Contador usa un lector comunitario sobre archivos generados por Fortnite. **No es oficial ni está aprobado por Epic Games; no se garantiza inmunidad a sanciones.**

### Aviso de Windows

<p align="center"><img src="docs/images/windows-smartscreen.png" width="535" alt="Windows SmartScreen muestra Contador como una aplicación con editor desconocido"></p>

El ejecutable no tiene firma de editor verificado. Windows puede mostrar **Windows protegió su PC** o **Editor desconocido** por reputación insuficiente. El aviso no demuestra que haya un virus, ni garantiza que el archivo sea seguro. No necesitas desactivar el antivirus, crear exclusiones ni instalar certificados del proyecto.

Si aparece una detección concreta o no puedes comprobar su origen, no lo ejecutes y [comparte el resultado](https://github.com/Axxon-g/contador-cofres-fornite/issues). [Información de firma](docs/FIRMA.md).

### Comprueba el archivo que descargues

Puedes subir **tu copia del ejecutable** a [VirusTotal](https://www.virustotal.com/) antes de abrirla. El [análisis compartido anteriormente](https://www.virustotal.com/gui/file/f0ebdf6c9a65c57b66b97957ab37574e96f32c136201fedcd97be4bb05e2a1c6) corresponde a otro archivo y **no acredita esta compilación 2.0**. Ningún resultado aislado garantiza ausencia de malware.

Para calcular su huella en PowerShell:

```powershell
Get-FileHash .\Contador.exe -Algorithm SHA256
```

Compara el resultado con la huella publicada para esa misma Release, si está disponible. La coincidencia verifica la identidad del archivo, no su seguridad por sí sola. El código y las dependencias están disponibles para revisión.

## Preguntas frecuentes

<details>
<summary><strong>¿Por qué no se actualiza al abrir cada cofre?</strong></summary>

El historial depende de que Fortnite finalice la repetición. El conteo visual opcional observa avisos XP/PE y es aproximado; no puede ver aperturas que no produzcan un aviso compatible.
</details>

<details>
<summary><strong>¿Funciona en consolas o GeForce NOW?</strong></summary>

Esta versión es una app de Windows que necesita repeticiones locales. No obtiene esos archivos automáticamente desde una consola ni desde el equipo remoto de GeForce NOW. Introducir un nombre de usuario no recupera sus cofres mediante una API pública.
</details>

<details>
<summary><strong>¿Por qué mi equipo muestra menos cofres de los que abrió?</strong></summary>

Las repeticiones pueden omitir acciones de compañeros, especialmente lejanas. Importar sus perspectivas mejora la cobertura; no convierte el registro en un total exacto del servidor. Revisa la columna Origen y el indicador ≥.
</details>

<details>
<summary><strong>¿Puedo recuperar mi historial anterior?</strong></summary>

La app busca historiales locales de ChestCounter y ChestTracker, incorpora partidas sin duplicarlas y conserva respaldos. No sincroniza equipos ni recupera archivos eliminados de Fortnite. Puedes elegir la cuenta en la cabecera y revisar las fechas en Ajustes.
</details>

<details>
<summary><strong>¿Qué hago si no aparece ningún cofre o falla un replay?</strong></summary>

Comprueba Grabar repeticiones, carpeta, cuenta y período. Deja terminar la partida y revisa Ajustes → Registro de errores. Una actualización del formato de Fortnite puede requerir cambios del lector; no toda repetición es compatible.
</details>

## Compilar desde el código

Necesitas el **SDK de .NET 10** en Windows x64. Conserva `vendor`: contiene el lector y las bibliotecas necesarias. El SDK no es necesario para ejecutar la descarga ya compilada.

```powershell
git clone https://github.com/Axxon-g/contador-cofres-fornite.git
cd contador-cofres-fornite
.\build.bat
```

El script comprueba el motor y los controles de escritorio y genera **`dist\2.0.1\Contador.exe`**. Cierra la app antes de sustituir ese archivo. La publicación es autocontenida; no hace falta distribuir los archivos de código ni ejecutar scripts en el equipo del usuario.

```text
src/Contador/              Interfaz WPF, captura opcional y coordinación
src/Contador.Engine/       Reglas, fuentes, historial, informes y mapa
tests/                    Comprobaciones del motor y de escritorio
vendor/                   Lector y dependencias con sus licencias
docs/                     Uso, seguridad, investigación y versiones
```

- [Notas de la versión 2.0.1](docs/RELEASE-v2.0.1.md) · [Versión 2.0](docs/RELEASE-v2.0.0.md)
- [Arquitectura](docs/ARQUITECTURA.md) · [Verificación y límites](VERIFICACION.md)
- [Investigación de lectores y conciliación](docs/INVESTIGACION-v2.0.md)
- [Procedencia y licencias de dependencias](vendor/UPSTREAM.md)
- Versiones anteriores: [1.3.2](docs/RELEASE-v1.3.2.md), [1.3.1](docs/RELEASE-v1.3.1.md), [1.3](docs/RELEASE-v1.3.0.md).

## Créditos

Basado en el lector de [LED-N/Fortnite-ChestCounter](https://github.com/LED-N/Fortnite-ChestCounter). Las referencias adicionales y decisiones adoptadas están documentadas en la investigación; los avisos aplicables se conservan en `vendor`.

Fortnite y los recursos del juego pertenecen a sus titulares. Las imágenes aportadas se usan para identificar y mostrar este proyecto comunitario. La licencia MIT del código no concede derechos sobre esas marcas o recursos. El terreno HD es un servicio externo de Fortnite.GG.
