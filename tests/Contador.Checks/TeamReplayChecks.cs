using ChestCounter;
using Contador.Engine;

internal static class TeamReplayChecks
{
    public static void Run(Action<bool, string> check)
    {
        var mine = new MatchRecord
        {
            SessionId = "match-a",
            SessionIdentityVerified = true,
            EpicId = "me",
            ChestsOpened = 12,
            TeammateChestsOpened = 5,
            TeamChests = [new TeamChestRecord { EpicId = "mate", PlayerName = "Compañero", Chests = 5 }]
        };
        var friend = new MatchRecord
        {
            SessionId = "match-a",
            SessionIdentityVerified = true,
            EpicId = "mate",
            IsReplayOwner = true,
            AnalyzerVersion = ReplayAnalyzer.CurrentVersion,
            ChestsOpened = 20
        };
        var merged = TeamReplayReconciliation.Resolve(mine, [friend, friend]);
        check(merged.TeammateChests == 20 && merged.Sources == 1,
            "Dos copias del replay de un compañero sustituyen su mínimo una sola vez");
        var accounting = TeamAccounting.Create([mine], [friend]);
        check(accounting.TeamChests == 32 && mine.ChestsOpened == 12 && mine.TeammateChestsOpened == 5,
            "Conciliación no altera el historial propio ni el mínimo original");
        friend.SessionId = "match-b";
        check(!TeamReplayReconciliation.CanUse(mine, friend), "Otra sesión no se mezcla");
        friend.SessionId = "match-a";
        friend.EpicId = "enemy";
        check(!TeamReplayReconciliation.CanUse(mine, friend), "Otro equipo en la misma partida no se importa");
        friend.EpicId = "me";
        check(!TeamReplayReconciliation.CanUse(mine, friend), "El propio jugador no se suma como compañero");
        friend.EpicId = "mate";
        friend.SessionIdentityVerified = false;
        check(!TeamReplayReconciliation.CanUse(mine, friend), "Sesión inferida del nombre de archivo no se mezcla");
        friend.SessionIdentityVerified = true;
        friend.IsReplayOwner = false;
        check(!TeamReplayReconciliation.CanUse(mine, friend), "Una perspectiva ajena no se presenta como recuento propio");
        friend.IsReplayOwner = true;
        friend.AnalyzerVersion = 17;
        check(!TeamReplayReconciliation.CanUse(mine, friend), "Fuentes antiguas requieren nuevo análisis");
        friend.AnalyzerVersion = ReplayAnalyzer.CurrentVersion;
        friend.ChestsOpened = 3;
        var conflict = TeamReplayReconciliation.Resolve(mine, [friend]);
        check(conflict.TeammateChests == 5 && conflict.Conflicts == 1 && conflict.Sources == 0,
            "Una fuente contradictoria no resta ni infla el mínimo observado");
        mine.TeamChests[0].EpicId = null;
        check(!TeamReplayReconciliation.CanUse(mine, friend), "No confunde jugadores por compartir nombre");
        mine.TeamChests[0].EpicId = "mate";
        friend.ChestsOpened = 20;
        var newerPartial = new MatchRecord
        {
            SessionId = "match-a",
            SessionIdentityVerified = true,
            EpicId = "mate",
            IsReplayOwner = true,
            AnalyzerVersion = ReplayAnalyzer.CurrentVersion,
            ChestsOpened = 3,
            AnalyzedAtUtc = DateTime.UtcNow
        };
        var strongest = TeamReplayReconciliation.Resolve(mine, [friend, newerPartial]);
        check(strongest.TeammateChests == 20 && strongest.Members.Single().Chests == 20,
            "El detalle por jugador coincide con el total y conserva la perspectiva más completa");
        check(strongest.Members.Single().OwnPerspective, "El origen identifica la perspectiva del propio compañero");
        mine.TeamChests.Add(new TeamChestRecord { EpicId = "peer", PlayerName = "Otro compañero", Chests = 2 });
        mine.TeammateChestsOpened = 7;
        var peer = new MatchRecord
        {
            SessionId = "match-a",
            SessionIdentityVerified = true,
            EpicId = "peer",
            PlayerName = "Otro compañero",
            IsReplayOwner = true,
            AnalyzerVersion = ReplayAnalyzer.CurrentVersion,
            ChestsOpened = 10,
            TeamChests = [new TeamChestRecord { EpicId = "mate", PlayerName = "Compañero", Chests = 17 }]
        };
        var combined = TeamReplayReconciliation.Resolve(mine, [peer]);
        check(combined.TeammateChests == 27 && combined.Sources == 1,
            "Una perspectiva completa huecos de otros compañeros sin sumar duplicados");
        check(!combined.Members.First(member => member.EpicId == "mate").OwnPerspective,
            "Observar a un tercero sigue siendo parcial aunque llegue de un compañero");
        var direct = TeamReplayReconciliation.Resolve(mine, [friend, peer]);
        check(direct.TeammateChests == 30 && direct.Members.Sum(member => member.Chests) == 30,
            "El total combina las mejores cifras por jugador, no las sumas de archivos");
        peer.EpicId = "enemy";
        check(TeamReplayReconciliation.Resolve(mine, [peer]).TeammateChests == 7,
            "Un observador ajeno no puede aportar datos de terceros");
        check(TeamReplayReconciliation.SameSession("A58C5817-4862-47DE-A7C9-364027F6BEB8", "a58c5817486247dea7c9364027f6beb8"),
            "Reconoce el mismo identificador GUID sin depender de su representación");
        check(!TeamReplayReconciliation.SameSession("match-A", "match-a"), "No flexibiliza identificadores opacos arbitrarios");
    }
}
