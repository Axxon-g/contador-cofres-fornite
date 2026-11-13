# Mapa interactivo 43.10

`island-43.10.base64` contiene el PNG sin etiquetas de 2048 × 2048 píxeles servido por `https://fortnite-api.com/images/map.png?v=1inr9qg-20261113`. Se obtuvo el 13/11/2026 entre los recursos cargados por `https://lifecrisis.life/loot`. Se conserva sin modificar, codificado como texto para integrarlo en el ejecutable. SHA-256 del PNG original: `4f5d72efacc1ad692ee8eebc7e8a7c2cbf88f0e1bb55bf297a105e9c1dea34b1`. El arte de Fortnite pertenece a Epic Games; no implica aprobación oficial de esta app.

El catálogo público de Crisis declara versión 43.10 y actualización 12/11/2026. Se consultó el 13/11/2026. Se usan datos factuales de posición de píxel y clasificación de 2.145 posibles apariciones y 16 lugares. La extracción, clasificación y altura/planta de cada aparición no se han verificado independientemente en una partida. No se incorpora código, estilo, iconos ni ilustraciones del proveedor. No se afirma una licencia de datos abiertos. SHA-256 de `MapSpawns.json` integrado: `f326addfe144192e3d6874d48cac628c5a466134b08556d03d5c2b3da18bc4dc`.

`InteractiveMap` dibuja posiciones en el mismo marco de píxeles de ese PNG, con zoom, arrastre, agrupación y marcadores vectoriales propios. Las paradas editoriales usan los lugares del catálogo; las intermedias no comprobadas se indican como orientativas. Los números coral son paradas de la ruta manual; los números de los grupos de botín son cantidades de registros. La línea manual no es navegación ni predicción de premios. No hay peticiones de red para mostrar el mapa.

La imagen histórica aportada por el usuario, `treasure-hunters-map.base64` (375 × 375), permanece como archivo de trabajo anterior y ya no se integra en el ejecutable ni se usa como fondo para estas coordenadas.

Corrección visual posterior: `VaultReferences.json` sustituye únicamente los ocho registros vault de este proveedor por nueve anclas aproximadas digitalizadas de la imagen aportada por el usuario (511×600). La transformación y sus límites están documentados en docs/MAPA-INTERACTIVO-1.2.7.md. No altera ni certifica las otras categorías. El JSON original se conserva para auditar la sustitución.
