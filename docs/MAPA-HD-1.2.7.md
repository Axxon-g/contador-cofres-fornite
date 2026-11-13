# Terreno HD opcional · 1.2.7

## Qué cambia

El mapa propio sigue siendo un control nativo WPF. La opción **Terreno HD · Internet** superpone piezas de mayor resolución sobre la imagen integrada. El zoom admite 100–3200 %. Los grupos se van separando al acercar; los registros coincidentes siguen disponibles en el selector de puntos próximos, porque no existen datos fiables de planta o altura.

La opción está activada por defecto al iniciar. No requiere cuenta ni clave y puedes desactivarla desde la barra del mapa. Desactivarla cancela las peticiones pendientes. La imagen integrada, las capas, las rutas y el historial siguen disponibles sin conexión; las piezas guardadas se reutilizan al activar HD, incluso si falla la red.

## Investigación y fuente

- [Fortnite-API](https://fortnite-api.com/v1/map) ofrece las imágenes generales y nombres del mapa. La imagen integrada tiene 2048×2048 píxeles: ampliarla no añade detalle real.
- En el mapa público de [Fortnite.GG](https://fortnite.gg/) se observaron imágenes WebP de 256×256 con direcciones como `https://fortnite.gg/maps/43/6/29/34.webp`. Los niveles dividen el mismo cuadrado en una cuadrícula: nivel 3 equivale a 2048 píxeles y nivel 7 a 32768. El nivel solicitado depende de la escala en pantalla y del DPI.
- La [documentación de api-fortnite.com](https://prod.api-fortnite.com/swagger/index.html) anuncia otra API de teselas 32K. Su endpoint de mapas respondió que requiere clave. Es un proveedor distinto de fortnite-api.com; no se utiliza una clave de otra API ni se integra ninguna credencial en el ejecutable.
- No se ha identificado una API pública oficial de Epic que proporcione este servicio HD al contador. La [Fortnite Data API de Epic](https://dev.epicgames.com/documentation/en-us/fortnite/using-fortnite-data-api-in-fortnite) describe métricas agregadas de islas, no este servidor de imágenes.

La app usa código propio para dibujar las piezas; no incorpora el código, interfaz, anuncios, cookies ni iconos de Fortnite.GG. No distribuye una copia de sus teselas dentro del ejecutable. La atribución y el enlace al proveedor permanecen visibles. El arte y servicio conservan los derechos de sus titulares; los [términos de Fortnite.GG](https://fortnite.gg/legal/terms) no se presentan como una licencia de redistribución ni como una API con disponibilidad garantizada.

## Red, caché y límites

Solo se solicitan imágenes HTTPS del mapa 43 en fortnite.gg. El proveedor recibe las peticiones de imágenes y la información habitual de conexión, como la IP; no se le envían credenciales, historial, repeticiones ni estadísticas. Se usa un identificador de cliente Contador/1.2.7, sin simular una sesión de navegador ni reenviar cookies.

Se permiten cuatro cargas simultáneas, ocho segundos de espera por petición y dos minutos antes de reintentar una pieza fallida. Los cambios de vista descartan solicitudes pendientes obsoletas. Cada pieza debe ser WebP de 256×256, con un máximo de 2 MB; no se aceptan páginas HTML como imágenes. La memoria conserva hasta 96 piezas; la caché de disco se limita a 128 MB en `%LOCALAPPDATA%/ContadorModern/MapTiles/43`.

Los errores HTTP, ausencia de conexión, falta de espacio o de decodificador WebP en Windows mantienen el fondo integrado y muestran el estado. No se eluden bloqueos del proveedor. Las piezas pueden llegar por etapas; durante la carga puede verse una mezcla de detalle HD y fondo de menor resolución. Las imágenes de pruebas con caché parcial se identifican como tales.

El servidor puede modificar o retirar las imágenes. El catálogo comunitario de posiciones sigue siendo una instantánea 43.10 y las bóvedas son referencias visuales aproximadas; mejorar la resolución no certifica su exactitud ni actualiza esos datos automáticamente.

## Verificación

Pruebas de niveles, DPI, índices y encuadre en `--map-only`; controles de activación en `--map-ui-only`. `--tile-cache-only <archivo.webp>` comprueba decodificación de una pieza real, rechazo de HTML, recuperación sin red y respaldo cuando falta una pieza. Las capturas se realizan con una caché de prueba aislada, sin tocar el historial ni conectar al proveedor durante el render.

Se pudo obtener una pieza real de nivel 6 durante la investigación. La descarga de otras piezas desde las herramientas de esta sesión fue irregular; no se certifica la disponibilidad de una sesión HD completa ni que el proveedor acepte todas las peticiones del cliente de escritorio. El modo se mantiene opcional con respaldo local.
