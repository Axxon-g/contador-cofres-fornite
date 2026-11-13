# Rutas sugeridas e informe nativo · 1.2.7

Revisión editorial: 13 de noviembre de 2026. Se retira la pantalla de análisis espacial del historial y su proyección de coordenadas. Los registros y los recuentos existentes se conservan; no se borra el historial ni se vuelve a migrar su formato. Los campos opcionales guardados anteriormente siguen siendo compatibles.

## Qué demuestra la investigación

Se consultaron anuncios de Epic, publicaciones y comentarios de Reddit, una recopilación de rutas de creadores en X y una guía de localizaciones. No se encontró un conjunto de intentos y premios comparable entre lugares que permita decir dónde se obtienen más recompensas. La selección prioriza recorridos publicados, concentración de lugares de búsqueda y claridad de la fuente, sin clasificarlos por probabilidades de premio.

| Propuesta | Evidencia y uso en la aplicación |
|---|---|
| The Chest → islote al sur → Arlong Park | Recorrido propuesto por un usuario; otro relato menciona The Chest al repartir su escuadrón. No prueba mayor tasa de premios. |
| Lantern Landing | Relato individual de un hallazgo y su lugar habitual de aterrizaje. Se presenta como testimonio, sin estimar probabilidades. |
| Norte de Plunder Port | Recopilación periodística de circuitos de creadores. Las publicaciones enlazadas de X no pudieron leerse directamente (403); la app lo declara. |
| Windmill Village → Loguetown | Rotación propuesta por una guía de localizaciones, sin relato de hallazgo asociado ni cifras garantizadas. |

### Fuentes

- [Asta_zzz en Reddit: recorrido The Chest–Arlong Park](https://www.reddit.com/r/FortniteBR/comments/1xloucg/im_logging_off_and_coming_back_tmrw_also_i_got/), publicación del 02/11/2026.
- [nothiinq en Reddit: relato y discusión de rutas](https://www.reddit.com/r/FortniteBR/comments/1xn5kb8/i_just_found_the_token_after_a_24h_grind_and/), 05/11/2026. Los comentarios también describen competencia por cofres y resultados dispares.
- [Puzzleheaded-Arm-988 en Reddit: testimonio de Lantern Landing](https://www.reddit.com/r/FortniteBattleRoyale/comments/1xu6c4w/i_found_the_one_piece_its_real/), 12/11/2026.
- [GamesRadar+: rutas compartidas por creadores](https://www.gamesradar.com/games/fortnite/fortnite-players-desperate-for-free-skins-have-mapped-out-the-fastest-routes-to-grind-for-the-pirate-shop-token-because-yes-it-really-is-that-serious/), 04/11/2026. Se usa como fuente secundaria, no como validación de resultados.
- Publicaciones de X enlazadas por el reportaje: [circuitos](https://twitter.com/cantworkitout/status/2117229081632915701) y [reparto para escuadrones](https://twitter.com/cantworkitout/status/2117692232165892473). No verificadas directamente.
- [Burnaviour: Windmill Village](https://burnaviour.dev/places/windmill-village/), actualizada el 12/11/2026. Los conteos de esta guía usan un radio alrededor del centro; por eso no se mezclan con conteos de otras páginas ni se prometen como botín disponible.
- [Epic Games: anuncio de Treasure Hunters](https://www.fortnite.com/news/fortnite-treasure-hunters-the-greatest-adventure-of-all-time), 30/10/2026: contexto del evento y Treasure Hours. La app no muestra horarios en directo ni inventa tasas.

Las descripciones son paráfrasis y los enlaces se abren solo a petición del usuario. No se copian publicaciones completas ni imágenes de otros jugadores. La imagen del mapa es la ya aportada por el usuario e integrada en el ejecutable; cada ruta añade su propio diagrama orientativo de sectores. No es una visualización de recompensas observadas ni un detector de cofres.

## Informe dentro de la app

El nuevo apartado usa la misma cuenta, fechas y reglas de recuento que el HTML: total propio, sesión, media, récord, tesoros, últimos 30 días, desglose por tipo, interacciones fuera del total, orígenes y fiabilidad, detalle de hallazgos y últimas 100 partidas. Una casilla permite ver el equipo sin sumarlo a los totales propios. Se declara la cobertura cuando un historial importado tiene cifras sin desglose. Los días del gráfico también pueden leerse en una tabla.

El informe se recalcula al terminar un análisis o cambiar cuenta y período. No se accede a la cuenta de Epic ni se envía el historial a las fuentes de las guías. La versión HTML sigue disponible mediante un botón específico.

## Comprobación

`dotnet run --project tests/Contador.Checks -- --report-only` comprueba agregados, fechas, cuenta, sesión, separación del equipo, correspondencia con el HTML, estados vacíos, cobertura parcial e integridad de las guías. La vista aislada con datos ficticios se abre con `dotnet run --project tests/Contador.DesktopChecks -- --views-preview` y no lee ni escribe el historial del usuario.
