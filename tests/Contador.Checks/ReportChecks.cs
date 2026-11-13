using ChestCounter;
using Contador.Engine;

internal static class ReportChecks
{
    public static void Run(Action<bool, string> check)
    {
        var date = new DateTime(2026, 11, 2, 18, 0, 0, DateTimeKind.Utc);
        var data = new TrackerData
        {
            Matches = [
                new()
                {
                    EpicId = "A", SessionId = "a1", PlayerName = "Uno", MatchStartUtc = date,
                    AnalyzedAtUtc = date, ChestsOpened = 12, Placement = 1,
                    Chests = [new() { Kind = "Coffre" }, new() { Kind = "Coffre", Destroyed = true }],
                    OtherInteractions = new() { [ChestKinds.AmmoBox] = 4 },
                    TreasureFinds = [new() { Item = "propio", SourceKind = "Coffre", SourceCertain = true,
                        FromGoldenChest = true, TimeExact = true, ObtainedAtSeconds = 85 }],
                    TeamTreasureFinds = [new() { Item = "equipo", SourceKind = "Coffre", Teammate = "Otro" }]
                },
                new() { EpicId = "A", SessionId = "a2", MatchStartUtc = date.AddDays(-1),
                    AnalyzedAtUtc = date.AddDays(-1), ChestsOpened = 8, AmmoBoxesOpened = 3 },
                new() { EpicId = "A", SessionId = "old", MatchStartUtc = date.AddDays(-10), ChestsOpened = 99 },
                new() { EpicId = "B", SessionId = "b", MatchStartUtc = date, ChestsOpened = 500 }
            ]
        };
        var config = new Config { Profile = "A", MatchDateFrom = new DateTime(2026, 11, 1) };
        var session = new SessionCounter(date.AddHours(-1));
        var report = PlayerReport.Create(data, config, session, date.ToLocalTime());
        check(report.Total == 20 && report.Matches == 2, "Informe separa cuenta y período");
        check(report.Average == 10 && report.Best == 12, "Media y récord usan todas las partidas filtradas");
        check(report.Session == 12, "Sesión excluye partidas antiguas");
        check(report.Treasures == 1 && report.TeamTreasures == 1, "No mezcla tesoros propios con equipo");
        check(report.Finds.Count(f => !f.Team) == 1 && report.Finds.Count(f => f.Team) == 1,
            "Equipo puede mostrarse sin modificar totales");
        check(report.Finds.Single(f => !f.Team).Moment == "1:25", "Momento conserva el reloj del HTML");
        check(report.Finds.Single(f => f.Team).Who.Contains("estimación"), "Equipo se marca estimado");
        check(report.Kinds.Sum(k => k.Count) == 2 && report.Coverage.Contains("20"),
            "Historial sin detalle conserva total y declara cobertura parcial");
        check(report.Other.Sum(k => k.Count) == 7 && report.Total == 20, "Munición no suma cofres");
        check(report.Sources.Sum(s => s.Count) == 1, "Orígenes excluyen tesoros del equipo");
        check(report.Days.Count == 30 && report.Days.Sum(d => d.Count) == 20, "Treinta días usan el mismo filtro");
        check(report.Days.Max(d => d.Height) == 120 && report.Days.All(d => d.Height >= 3),
            "Gráfico admite ceros y valores positivos");
        var before = ReportWriter.SessionFilter;
        try
        {
            ReportWriter.SessionFilter = session.Includes;
            var html = ReportWriter.Report(config, Profiles.View(data, config), Profiles.List(data, config));
            check(html.Contains(">20</div>") && html.Contains(">10,0</div>") && html.Contains("12 desde que se abrió"),
                "Totales, media y sesión coinciden con el HTML");
        }
        finally { ReportWriter.SessionFilter = before; }
        var empty = PlayerReport.Create(data,
            new Config { Profile = "A", MatchDateFrom = new DateTime(2027, 1, 1) }, session, date);
        check(empty.Total == 0 && empty.Finds.Count == 0 && empty.Kinds.Count == 0,
            "Cuenta vacía no hereda datos previos");
        check(empty.Days.Count == 30 && empty.Days.All(d => d.Count == 0), "Informe vacío conserva días a cero");
        config.Profile = "B";
        check(PlayerReport.Create(data, config, session, date).Total == 500, "Cambio de cuenta recalcula informe");
        check(data.Matches.Count == 4 && data.Matches[0].ChestsOpened == 12, "El informe no modifica el historial");
        check(RouteGuides.All.Count == 4, "Cuatro propuestas editoriales independientes");
        check(RouteGuides.All.All(g => g.Sources.Count > 0 && !string.IsNullOrEmpty(g.Caution)),
            "Cada propuesta declara fuentes y límites");
        check(RouteGuides.All.SelectMany(g => g.Stops).All(s => s.U is >= 0 and <= 1 && s.V is >= 0 and <= 1),
            "Marcadores orientativos dentro de la imagen");
        check(RouteGuides.All.SelectMany(g => g.Sources).All(s => Uri.TryCreate(s.Url, UriKind.Absolute, out var uri)
            && uri.Scheme == Uri.UriSchemeHttps), "Fuentes son enlaces HTTPS");
        check(RouteGuides.Notice.Contains("No hay datos comparables"), "No se inventa un ranking de recompensas");
    }
}
