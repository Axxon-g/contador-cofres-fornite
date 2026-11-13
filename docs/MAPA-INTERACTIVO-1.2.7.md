# Mapa interactivo nativo · Contador 1.2.7

El usuario eligió un mapa propio en WPF. Se toma Fortnite.GG como referencia de interacción, sin incrustar su página ni reutilizar su código, iconos o catálogo. El informe nativo y el lector de repeticiones mantienen sus reglas.

## Datos integrados

- Fuente: [Crisis, Loot spawns](https://lifecrisis.life/loot), versión declarada 43.10, actualización 12/11/2026, consulta 13/11/2026.
- Se conserva una instantánea de 2.145 registros publicados: cofres 925, raros 40, enterrados 69, munición 879, carteles 141, barriles de pesca 47, cañones 28, santuarios 8, bóvedas/búnkeres 8. Se añaden 16 lugares publicados como referencias separadas.
- `MapSpawns.json` contiene coordenadas de píxel y clasificación factual, no código del proveedor. Se omite el indicador de aparición fija porque no se interpreta ni se promete una probabilidad individual. Los registros coincidentes se conservan con identificadores distintos; no se deducen plantas o alturas.
- Imagen de 2048 × 2048 píxeles obtenida de Fortnite-API, en la misma sesión de consulta del mapa fuente. Los puntos se dibujan directamente en ese marco de píxeles; no se aplica una calibración afín ni se usan posiciones del historial.
- Instantánea fija, identificada por `crossbones-43.10-20261113`. Una versión distinta no hereda marcas de otra isla. No hay descarga ni actualización automática durante el uso de la app.
- Coordenadas comunitarias declaradas por el proveedor como extraídas del juego, sin comprobación independiente de cada punto en una partida. No se garantiza presencia de botín ni un premio. No se afirma una licencia de datos abiertos, aprobación de Epic ni derechos sobre el arte de Fortnite.

## Interacción

Zoom 100–800 % anclado al cursor, arrastre, botones, flechas e Inicio. Los grupos conservan el número de registros y permiten acercarse; al máximo, el selector de puntos próximos distingue registros coincidentes. Las capas, búsqueda y ocultación de revisados comparten el mismo filtro con la lista accesible.

La ruta admite hasta 100 paradas. Se añaden sobre el mapa o desde un punto seleccionado; se pueden mover, quitar o vaciar. Deshacer conserva las últimas 30 ediciones y también recupera las marcas de revisión. Las líneas son un orden de visita manual, no navegación, predicción o evitación de enemigos. Las propuestas editoriales anteriores pueden cargarse y modificarse; las referencias intermedias no verificadas llevan la indicación orientativa.

Las marcas Revisado/Vacío/Pendiente son planificación manual. Se guardan con las capas y ruta en `%APPDATA%/ContadorModern/mapa-crossbones-43.10-20261113.json`, separado de los datos del contador. Si el archivo local está dañado, se evita sobrescribirlo y se informa al usuario. Los errores de guardado se muestran sin afectar al historial. El modo de edición se desactiva al volver a abrir la app.

## Comprobación reproducible

- `dotnet run --project tests/Contador.Checks -- --map-only`: catálogo, categorías, versiones, coordenadas, transformación inversa, zoom al cursor, límites, agrupación sin perder registros, edición, deshacer y formato JSON.
- `dotnet run --project tests/Contador.DesktopChecks -- --map-ui-only`: controles reales WPF en una ventana aislada, sin escritura en el historial del usuario.
- `dotnet run --project tests/Contador.DesktopChecks -- --render-views <carpeta>`: vistas de mapa completo, reducido, ruta, zoom y filtro vacío, además del informe con datos ficticios. No enfoca la ventana ni interactúa con una partida.

## Corrección de bóvedas · 13/11/2026

La imagen de referencia aportada por el usuario muestra nueve bóvedas. Contradice las ocho posiciones `vault` del proveedor: se excluyen esos ocho registros al cargar, sin alterar el JSON original ni desplazar las demás capas. Se añaden nueve referencias visuales aproximadas desde `VaultReferences.json`; el total mostrado pasa a 2.146. La imagen no identifica su autor o URL original, por lo que no se presenta como fuente oficial ni se distribuye como recurso de la app.

Alineación de terreno entre referencia de 511×600 y PNG integrado de 2048×2048: X = 2,1225·x + 539,5; Y = 2,1475·y + 502,5. Ajuste por comparación de color sobre 4.857 muestras terrestres, con pérdida robusta para etiquetas e iconos. El error de la digitalización y el tamaño de los candados impiden afirmar una entrada exacta. No son coordenadas del juego. Cada referencia coincide a menos de 16 píxeles del PNG con un grupo del catálogo independiente de cofres raros; esto contrasta la alineación, no acredita la entrada de la bóveda.

Las bóvedas ya no se agrupan con otros tipos ni se mueven al centro de un grupo mixto. Sus nuevos identificadores no reutilizan revisiones de las ocho posiciones descartadas. Los identificadores de otras capas, las rutas manuales y el historial se conservan. Una ruta creada previamente por el usuario no se recalibra automáticamente.

Las demás ubicaciones siguen siendo comunitarias sin validar punto por punto. Esta corrección no certifica su precisión. Captura actual de la capa aislada: `docs/images/bovedas-corregidas.png`.
