namespace Contador.Engine;

public sealed record GuideStop(string Name, double U, double V, string Instruction);
public sealed record GuideSource(string Title, string Url);
public sealed record RouteGuide(
    string Title, string Evidence, string Summary, string Caution,
    IReadOnlyList<GuideStop> Stops, IReadOnlyList<GuideSource> Sources)
{
    public string Steps => string.Join("\n\n", Stops.Select((stop, i) => $"{i + 1}. {stop.Name}\n{stop.Instruction}"));
}

/// <summary>Editorial suggestions, independent of replay data and never loot predictions.</summary>
public static class RouteGuides
{
    public const string Reviewed = "Revisado el 16/11/2026 · Treasure Hunters · Battle Royale / Cero construcción";
    public const string Notice = "Zonas de búsqueda, no zonas con premios garantizados. No hay datos comparables para clasificar dónde se han obtenido más recompensas. Los testimonios son individuales; busca más cofres sin descuidar la tormenta ni los combates.";
    public static IReadOnlyList<RouteGuide> All { get; } = Array.AsReadOnly<RouteGuide>([
        new(
            "The Chest → Arlong Park",
            "Ruta compartida en Reddit · 02/11/2026",
            "Un jugador propone empezar por los cofres cercanos de The Chest y continuar hacia el sur. Otro testimonio de hallazgo menciona The Chest como parte del reparto de su escuadrón.",
            "La llegada de otros equipos puede reducir los cofres disponibles. Continúa a Arlong Park solo si el círculo lo permite; el recorrido no mejora la probabilidad individual de un cofre.",
            [
                new("The Chest", 99d / 375, 103d / 375, "Busca los cofres de la isla y los enterrados que encuentres."),
                new("Islote al sur", 84d / 375, 139d / 375, "La publicación sugiere este paso intermedio. El marcador solo orienta la zona; no señala cofres exactos."),
                new("Arlong Park", 61d / 375, 175d / 375, "Continúa con las construcciones accesibles si la zona sigue siendo segura.")
            ],
            [
                new("Asta_zzz · recorrido y consejos (Reddit)", "https://www.reddit.com/r/FortniteBR/comments/1xloucg/im_logging_off_and_coming_back_tmrw_also_i_got/"),
                new("nothiinq · testimonio y comentarios (Reddit)", "https://www.reddit.com/r/FortniteBR/comments/1xn5kb8/i_just_found_the_token_after_a_24h_grind_and/")
            ]),
        new(
            "Lantern Landing · búsqueda compacta",
            "Testimonio de hallazgo · Reddit · 12/11/2026",
            "Un jugador que afirma haber encontrado el token cuenta que aterrizaba en Lantern Landing por la concentración de cofres y las bóvedas cercanas. Es una experiencia personal, no una tasa de éxito verificada.",
            "No se conocen sus intentos exactos ni los de quienes no obtuvieron premios. El mapa marca el lugar, no las bóvedas ni una ruta interior comprobada.",
            [new("Lantern Landing", 134d / 375, 154d / 375, "Recorre los edificios próximos antes de hacer desplazamientos largos. Evalúa las bóvedas cercanas según acceso, enemigos y tormenta.")],
            [new("Puzzleheaded-Arm-988 · relato del hallazgo (Reddit)", "https://www.reddit.com/r/FortniteBattleRoyale/comments/1xu6c4w/i_found_the_one_piece_its_real/")]),
        new(
            "Al norte de Plunder Port",
            "Ruta de creadores · recogida por GamesRadar+ · 04/11/2026",
            "GamesRadar+ recoge un circuito de MacFNyt al norte de Plunder Port y propuestas de Gatocarls15 para repartir la búsqueda entre compañeros. La idea es reducir desplazamientos y cubrir distintas zonas.",
            "Los números de cofres difundidos son estimaciones, no botín asegurado. Las publicaciones de X no pudieron comprobarse directamente; se enlazan junto al reportaje. El mapa solo señala el sector norte.",
            [new("Sector norte de Plunder Port", 214d / 375, 286d / 375, "Revisa los grupos de construcciones al norte del puerto y vuelve hacia una salida segura. Si juegas en equipo, acuerda zonas para evitar revisar lo mismo.")],
            [
                new("GamesRadar+ · recopilación de rutas de creadores", "https://www.gamesradar.com/games/fortnite/fortnite-players-desperate-for-free-skins-have-mapped-out-the-fastest-routes-to-grind-for-the-pirate-shop-token-because-yes-it-really-is-that-serious/"),
                new("Publicación de ruta enlazada por el reportaje (X)", "https://twitter.com/cantworkitout/status/2117229081632915701"),
                new("Publicación para escuadrones enlazada por el reportaje (X)", "https://twitter.com/cantworkitout/status/2117692232165892473")
            ]),
        new(
            "Windmill Village → Loguetown",
            "Guía de localizaciones · Burnaviour · 12/11/2026",
            "La guía de Windmill Village propone revisar sus cofres y rotar después a Loguetown. Es una opción basada en cercanía y lugares de búsqueda, no en testimonios de una mayor tasa de premios.",
            "Los puntos de aparición pueden cambiar y no todos los cofres estarán disponibles. La línea une lugares: no representa un camino navegable ni evita enemigos.",
            [
                new("Windmill Village", 88d / 375, 266d / 375, "Revisa las construcciones próximas y los cofres enterrados que encuentres; prioriza los accesos seguros."),
                new("Loguetown", 164d / 375, 251d / 375, "Tras saquear la zona inicial, rota si la tormenta y el resto de equipos lo permiten.")
            ],
            [new("Burnaviour · Windmill Village y rotación cercana", "https://burnaviour.dev/places/windmill-village/")]),
        new(
            "Lantern Landing → The Chest → Lantern Landing",
            "Recorrido compartido por RubyEii · Reddit · consultado el 16/11/2026",
            "Una propuesta de la comunidad combina Lantern Landing, el sector de la bóveda cerca de The Chest y el regreso al lugar inicial. Las paradas indican sectores; consulta el recorrido original para los pasos interiores.",
            "Los resultados publicados son personales. Las bóvedas, jardines y torres no están geolocalizados en esta guía. Ajusta la ruta al círculo, los accesos y los enemigos; no se garantiza un número de cofres ni premios.",
            [
                new("Lantern Landing", 0, 0, "Empieza por las construcciones próximas y revisa los accesos seguros."),
                new("The Chest", 0, 0, "Referencia del sector: el autor propone la bóveda cercana y jardines. Ajusta las paradas manualmente a los accesos que conozcas."),
                new("Lantern Landing", 0, 0, "Regresa solo si quedan cofres disponibles y la tormenta permite continuar.")
            ],
            [new("RubyEii · circuito y discusión (Reddit)", "https://www.reddit.com/r/AllFortnite/comments/1xrf05a/how_to_average_50_chests_per_match_a_guide_with/")]),
        new(
            "Cursed Cavern · explorar el interior",
            "Recorrido de PurpleDuctape · Reddit · consultado el 16/11/2026",
            "El autor recomienda comenzar dentro de Cursed Cavern, revisar sus zonas ocultas y continuar por la salida sur. Esta opción marca el área inicial: los pasos interiores se describen en la fuente y puedes dibujarlos en tu ruta.",
            "Sin datos de altura, la vista del mapa no distingue cuevas, techos ni plantas. No copiamos posiciones interiores sin verificar ni prometemos cofres disponibles. Prioriza salir antes de la tormenta.",
            [new("Cursed Cavern", 0, 0, "Comienza en el interior. Revisa accesos y salida sur; amplía la ruta manualmente cuando conozcas el terreno.")],
            [new("PurpleDuctape · recorrido explicado (Reddit)", "https://www.reddit.com/r/FortniteBR/comments/1xmtpaz/heres_some_help_with_the_chest_hunt/")])
    ]);

    public static GuideSource Official { get; } = new(
        "Epic Games · Treasure Hunters y Treasure Hours",
        "https://www.fortnite.com/news/fortnite-treasure-hunters-the-greatest-adventure-of-all-time");
}
