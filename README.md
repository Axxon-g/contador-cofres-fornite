# Contador

Aplicación de escritorio para Windows 10/11 x64. Interfaz WPF en español, con resumen, historial buscable, exportación CSV, perfiles por cuenta y overlay para jugar o transmitir.

## Compilar y ejecutar

1. Instala el **SDK de .NET 10 para Windows x64**: https://dotnet.microsoft.com/download/dotnet/10.0
2. Ejecuta `build.bat`. Requiere internet en la primera compilación para restaurar los paquetes y el runtime.
3. Abre `dist/Contador.exe`. El ejecutable publicado es autónomo: el equipo donde se use no necesita instalar .NET.
4. En Fortnite activa **Ajustes → Juego → Repeticiones → Grabar repeticiones**.
5. Deja Contador abierto. También analiza repeticiones existentes al iniciar.

La carpeta predeterminada es `%LOCALAPPDATA%\\FortniteGame\\Saved\\Demos`. Puedes cambiarla desde Ajustes.

## Cómo cuenta

### Historial anterior integrado

Desde la versión 1.1, abrir `Contador.exe` busca automáticamente los historiales de `%APPDATA%\ChestCounter` y `%APPDATA%\ChestTracker`. Incorpora las partidas faltantes y conserva el registro del analizador más reciente cuando hay una coincidencia. Reabrir la app no duplica partidas. Los perfiles siguen separados por cuenta.

Antes de guardar cambios crea copias de seguridad en `%APPDATA%\ContadorModern\Backups`. Los historiales de las otras aplicaciones se leen sin modificarlos. Si un archivo es incompatible o está incompleto, la app conserva sus datos actuales, informa del problema y vuelve a intentarlo al siguiente inicio. Si no hay un contador anterior, funciona normalmente desde cero.

Los usuarios finales solo necesitan descargar y abrir el ejecutable autónomo. No necesitan instalar .NET, ejecutar scripts, compilar ni copiar archivos de historial. `build.bat` y el SDK son herramientas para quien desarrolla y publica la app.

Usa el lector y el criterio de recuento de [LED-N/Fortnite-ChestCounter](https://github.com/LED-N/Fortnite-ChestCounter), fijado al commit `1f387987737cfd606611d5b8d294d5db04af9eab`.

Lee archivos .replay en solo lectura. No accede al proceso, memoria, credenciales ni instalación del juego. Fortnite entrega la clave de descifrado al terminar la partida; por ello los cofres se cuentan después, no instantáneamente al abrirlos. El lector separa cuentas por Epic ID, omite modos que no son Battle Royale, distingue cofres de cajas de munición y evita duplicar partidas renombradas.

Las partidas incompletas o incompatibles pueden no contarse. Una actualización de Fortnite puede requerir actualizar el lector. El registro en Ajustes permite diagnosticar errores. No se promete compatibilidad futura ni aprobación por Epic Games.

## Uso

- **Resumen:** total de la cuenta elegida, cofres incorporados en esta sesión, última partida, media y récord.
- **Historial:** buscar por fecha o modo, ordenar columnas y seleccionar una partida para ver su detalle.
- **Exportar CSV:** exporta las filas del filtro y perfil visibles.
- **Pausar detección:** detiene los siguientes análisis; uno ya iniciado termina antes.
- **Ajustes:** carpeta, cuenta, overlay y su tamaño. Las preferencias y el historial se guardan automáticamente.
- **Overlay:** actívalo desde Ajustes. Ctrl+Mayús+O muestra/oculta; Ctrl+Mayús+L bloquea/desbloquea. Desbloqueado se puede arrastrar. Fortnite debe usar Pantalla completa en ventana.
- **OBS:** agrega `overlay.html` de la carpeta de datos como archivo local de una fuente Navegador.
- **Cerrar:** termina el análisis actual y guarda antes de salir. Minimizar conserva la app en la barra de tareas.

La sesión incluye partidas importadas durante esta ejecución. El total histórico y el overlay corresponden a la cuenta seleccionada. Las cifras parten de cero: no se incluyen partidas de demostración.

## Datos y privacidad

Datos propios en `%APPDATA%\\ContadorModern`: `data.json`, `config.json`, `errors.log`, `report.html`, `overlay.html` y `matches.csv`. No toca los datos de la aplicación original. Para respaldar el historial, copia esa carpeta con la app cerrada. La app no activa actualizaciones automáticas ni envía estadísticas.

## Desarrollo y comprobaciones

```powershell
dotnet run --project src/Contador/Contador.csproj
dotnet run --project tests/Contador.Checks/Contador.Checks.csproj -c Release
.\\build.ps1
```

Las comprobaciones cubren cabeceras de repetición (en curso, finalizada, sin cifrar, UTF-16, v7, inválida y ausente), aislamiento de perfiles y persistencia. No sustituyen probar una repetición real ni comprobar la interfaz ejecutando la app.

## Créditos

El motor incluido bajo `vendor/ChestCounter` procede de LED-N/Fortnite-ChestCounter (MIT). Sus lectores derivan de FortniteReplayDecompressor de Shiqan y OozSharp; se conservan sus licencias y notas. La interfaz nueva y las comprobaciones se distribuyen bajo MIT. Fortnite es una marca de Epic Games; este proyecto no está afiliado a Epic.

El informe heredado usa fuentes de sistema en esta distribución; no requiere descargar fuentes.

