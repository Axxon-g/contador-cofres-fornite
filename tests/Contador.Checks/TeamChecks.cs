using System.Text.Json;
using ChestCounter;
using Contador.Engine;

internal static class TeamChecks
{
    public static void Run(Action<bool, string> check)
    {
        var now = DateTime.UtcNow;
        var known = new MatchRecord
        {
            SessionId = "known", EpicId = "A", ChestsOpened = 11, TeammateChestsOpened = 24,
            MatchStartUtc = now,
            TreasureFinds = [
                new() { SourceKind = ChestKinds.Normal, SourceCertain = true, FromGoldenChest = true },
                new() { SourceKind = ChestKinds.Rare, SourceCertain = true, FromPrizeHandler = true },
                new() { SourceKind = "inconnu", SourceIsChest = true },
                new() { SourceKind = ChestKinds.AmmoBox, SourceIsChest = false }
            ],
            TeamTreasureFinds = [
                new() { SourceCertain = true, Teammate = "Compañero", FromGoldenChest = true },
                new() { SourceCertain = false, Teammate = "" }
            ]
        };
        var legacy = new MatchRecord
        {
            SessionId = "legacy", EpicId = "A", ChestsOpened = 70, MatchStartUtc = now.AddDays(-1)
        };
        var other = new MatchRecord
        {
            SessionId = "other", EpicId = "B", ChestsOpened = 999, TeammateChestsOpened = 999,
            MatchStartUtc = now.AddMinutes(1)
        };
        var data = new TrackerData { Matches = [known, legacy, other] };
        var config = new Config { Profile = "A" };
        var selected = Profiles.View(data, config);
        var team = TeamAccounting.Create(selected.Matches);
        check(team.TeamChests == 105 && team.OwnChests == 81 && team.OwnCoveredChests == 11 && team.TeammateChests == 24,
            "Mínimo conjunto usa el mismo período personal, sin perder partidas antiguas ni mezclar cuentas");
        check(team.CoveredMatches == 1 && team.Matches == 2, "Legado sin compañeros no se convierte en cero conocido");
        check(team.OwnChestRewards == 1 && team.OwnProbableChestRewards == 1 && team.OwnOtherRewards == 2,
            "SourceCertain histórico no convierte atribuciones temporales en evidencia directa");
        check(team.OwnRecordedRewards == 4, "El desglose de origen conserva todas las recompensas registradas");
        check(team.TeammateRewards == 1 && team.EstimatedTeammateRewards == 1,
            "Recompensas del equipo separan detecciones de estimaciones");
        var session = new SessionCounter(now.AddHours(-1));
        var overlay = session.Overlay(selected, false);
        check(overlay.Total == "81" && overlay.Last == "11" && overlay.LastTeamChests == "≥ 35",
            "Overlay añade equipo de última partida sin cambiar total personal");
        check(session.Overlay(selected, true).LastTeamChests == "≥ 35", "En partida conserva el equipo de última finalizada");
        check(TeamAccounting.LastTeamTotal(legacy) == null, "Última sin datos de equipo queda desconocida");
        check(TeamAccounting.LastTeamTotal(new MatchRecord { ChestsOpened = 8, TeammateChestsOpened = 0 }) == 8,
            "Cero compañeros detectados es un resultado conocido distinto de legado");
        check(TeamAccounting.LastTeamTotal(new MatchRecord { ChestsOpened = 8, TeammateChestsOpened = -1 }) == null,
            "No muestra conteos de equipo inválidos");
        var legacyOverlay = session.Overlay(new TrackerData { Matches = [legacy] }, false);
        check(legacyOverlay.LastTeamChests == null, "Overlay no inventa cifras antiguas");
        var empty = TeamAccounting.Create([]);
        check(empty.Matches == 0 && empty.CoveredMatches == 0, "Estado sin partidas");
        var html = ObsHud.Render(overlay);
        check(html.IndexOf("id=\"teamLast\"", StringComparison.Ordinal) > html.IndexOf("id=\"last\"", StringComparison.Ordinal)
            && html.IndexOf("id=\"teamLast\"", StringComparison.Ordinal) < html.IndexOf("id=\"session\"", StringComparison.Ordinal),
            "OBS sitúa equipo debajo de última y antes de sesión");
        check(html.Contains("teamLast") && html.Contains("Mínimo detectado"), "OBS usa mismo valor y explica cobertura");
        check(!config.ShowTeamStats && JsonSerializer.Deserialize<Config>("{}")?.ShowTeamStats == false,
            "Vista de equipo desactivada por defecto y compatible con configuración anterior");
        config.ShowTeamStats = true;
        check(JsonSerializer.Deserialize<Config>(JsonSerializer.Serialize(config))?.ShowTeamStats == true,
            "Preferencia del equipo se serializa");
        config.MatchDateFrom = now.ToLocalTime().Date;
        check(TeamAccounting.Create(Profiles.View(data, config).Matches).Matches == 1, "Equipo respeta el filtro de fechas");
        check(selected.Matches.Sum(m => m.ChestsOpened) == 81 && known.TreasureFinds.Count == 4,
            "Consultar equipo no modifica cifras ni recompensas propias");
        var reportedCase = Enumerable.Range(0, 264).Select(i => new MatchRecord
        {
            ChestsOpened = i == 0 ? 1301 : i == 59 ? 3531 : 0,
            TeammateChestsOpened = i < 59 ? i == 0 ? 1456 : 0 : null
        }).ToList();
        reportedCase[0].TreasureFinds = Enumerable.Range(0, 58).Select(i => new TreasureFind
        {
            SourceKind = i < 53 ? ChestKinds.Normal : ChestKinds.AmmoBox,
            SourceIsChest = i < 53, SourceCertain = true, FromPrizeHandler = true
        }).ToList();
        var reported = TeamAccounting.Create(reportedCase);
        check(reported.OwnChests == 4832 && reported.TeamChests == 6288
            && reported.TeammateChests == 1456 && reported.CoveredMatches == 59,
            "Regresión de cobertura: 4832 propios + 1456 compañeros, sin sustituir propios por 1301");
        check(reported.OwnRecordedRewards == 58 && reported.OwnProbableChestRewards == 53
            && reported.OwnOtherRewards == 5 && reported.OwnChestRewards == 0,
            "Regresión de atribución: 58 registros, 53 estimados de cofres y cinco de otro origen");
    }
}
