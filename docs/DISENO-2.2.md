# Interfaz de Contador 2.2

El resumen prioriza los cofres propios del perfil y período seleccionados, la sesión,
la media y las últimas partidas. El conteo visual tiene un botón visible desde el inicio y solicita consentimiento; no inicia
capturas automáticamente. Su estimación sigue separada del historial confirmado.

«Perfil público» consulta rendimiento de Fortnite; «Mi progreso» muestra el historial
local de cofres. Las eliminaciones por partida y el tiempo jugado se calculan con los
datos del período devuelto, sin solicitudes adicionales. Los valores ausentes se
muestran como raya, nunca como cero.

La navegación compacta, la barra roja, los fondos azules de saturación moderada y los acentos dorados comparten recursos
entre todas las pantallas. SummaryView encapsula el resumen y comunica acciones mediante
eventos; no lee archivos ni realiza solicitudes por su cuenta.

## Recursos gráficos

La moneda y el cofre existentes se conservan. La imagen de temporada
`src/Contador/Assets/treasure-hunters.jpg` procede del anuncio oficial de Epic Games:
https://www.fortnite.com/news/fortnite-treasure-hunters-the-greatest-adventure-of-all-time
Se incluye localmente como recurso, sin peticiones de imágenes al abrir el resumen.
Las referencias técnicas y licencias permanecen en documentación y avisos de terceros,
fuera del launcher.

## Actualizaciones sugeridas

Se consulta al iniciar, al volver a abrir la ventana desde la bandeja y mediante los botones de la barra lateral y Ajustes. La consulta solo
lee metadatos públicos de la última Release estable del repositorio oficial. Un aviso
no modal aparece cuando su versión es posterior a la instalada. «Ver actualización»
abre una dirección fija de GitHub en el navegador. No se descargan ni ejecutan archivos.

No se envían nombres de jugadores, repeticiones, rutas locales ni estadísticas. GitHub
recibe la conexión habitual y el identificador genérico de la app. La solicitud no usa
claves, cookies ni credenciales y no sigue redirecciones; tiene límite de 8 segundos y
256 KiB de respuesta. Un fallo de red no se presenta como «estás actualizado».

Contrato del proveedor: https://docs.github.com/en/rest/releases/releases#get-the-latest-release

El botón lateral muestra «Buscar actualizaciones» mientras no haya una versión nueva.
Durante la consulta muestra «Buscando…» y evita solicitudes simultáneas. Si hay una
nueva versión muestra «Nueva actualización» y abre las descargas oficiales al pulsarlo.
Si no la hay, un mensaje junto al botón confirma «Ya tienes la última versión».
Los errores de red se muestran como fallos de comprobación, no como una confirmación.
No se hacen comprobaciones periódicas mientras la app permanece abierta.

## Tesoros en bloques

La pantalla separa el perfil y sus cofres, los controles de simulación, la tabla de
probabilidades y la ayuda. Los dos primeros bloques se colocan en columnas cuando
hay espacio y se apilan en ventanas pequeñas. La tabla permite comparar recompensa,
tasa de referencia y probabilidad sin repetir frases ni barras a todo el ancho.
Las advertencias esenciales siguen visibles; la fórmula y los supuestos se pueden
desplegar. No se cambian las tasas, las fórmulas ni el historial.

## Bloques en Resumen, Mapa y Mi progreso

Resumen agrupa el conteo opcional, los totales, los indicadores, las últimas partidas
y la calidad del registro. El botón de conteo sigue visible sin desplegar controles.
Mi progreso separa cifras, gráfica diaria, tipos de cofre, origen de tesoros, detalle
de recompensas y últimas partidas. Las interacciones que no suman están desplegables.
El mapa separa la barra de controles, las capas, la selección y la planificación;
conserva el lienzo, la atribución del terreno y los mensajes de guardado visibles.
Los cambios se limitan a la presentación y conservan los enlaces y eventos existentes.
