# Capturas de las funciones 1.2.7

Capturadas el 13/11/2026 con la herramienta WPF del propio proyecto:

```powershell
dotnet run --project tests/Contador.DesktopChecks/Contador.DesktopChecks.csproj -c Release -- --render-views capturas
```

| Imagen publicada | Captura original | Contenido |
|---|---|---|
| mapa-interactivo.png | rutas-desktop.png | Isla, capas y grupos de puntos. |
| mapa-detalle.png | mapa-zoom.png | Zoom máximo y selección. |
| mapa-ruta.png | mapa-ruta.png | Recorrido manual numerado. |
| informe-nativo.png | informe-desktop.png | Informe con estadísticas ficticias. |

Son renders de controles nativos del código, sin retoques ni generación de imágenes. La barra superior identifica la herramienta de prueba y no forma parte de la aplicación distribuida. No incluyen historiales personales. El mapa utiliza el recurso y el catálogo integrados, cuya procedencia se documenta en [MAP-SOURCE.md](../../src/Contador/Assets/MAP-SOURCE.md).
