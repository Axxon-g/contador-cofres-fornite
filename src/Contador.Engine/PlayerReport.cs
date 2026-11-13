using ChestCounter;

namespace Contador.Engine;

public sealed record ReportCount(string Label, long Count, string Note = "");
public sealed record ReportDay(DateTime Day, long Count, double Height)
{
    public string Date => Day.ToString("dd/MM/yyyy");
    public string Description => $"{Date}: {Count:N0} cofres";
}
public sealed record ReportTreasure(string Date, string Name, string Moment, string Source, string Who, bool Team);
public sealed record ReportMatch(string Date, string Mode, string Placement, int Chests, string Treasures);

public sealed record PlayerReport
{
    public long Total { get; init; }
    public long Session { get; init; }
    public int Matches { get; init; }
    public double Average { get; init; }
    public int Best { get; init; }
    public int Treasures { get; init; }
    public int TeamTreasures { get; init; }
    public string Ratio => Treasures > 0
        ? $"1 cada {Math.Round((double)Total / Treasures):N0} cofres en tu historial; no es una probabilidad futura."
        : "Todavía no hay tesoros propios registrados.";
    public string Coverage { get; init; } = "";
    public string BestDay { get; init; } = "";
    public string Updated { get; init; } = "";
    public IReadOnlyList<ReportDay> Days { get; init; } = [];
    public IReadOnlyList<ReportCount> Kinds { get; init; } = [];
    public IReadOnlyList<ReportCount> Other { get; init; } = [];
    public IReadOnlyList<ReportCount> Sources { get; init; } = [];
    public IReadOnlyList<ReportTreasure> Finds { get; init; } = [];
    public IReadOnlyList<ReportMatch> Latest { get; init; } = [];

    /// <summary>Same profile/period and counting rules as the HTML report. Never mutates history.</summary>
    public static PlayerReport Create(TrackerData data, Config config, SessionCounter session, DateTime now)
    {
        var matches = Profiles.View(data, config).Matches
            .OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).ToList();
        var total = matches.Sum(m => (long)m.ChestsOpened);
        var chests = matches.SelectMany(m => m.Chests).ToList();
        var daily = matches.GroupBy(m => (m.MatchStartUtc ?? m.AnalyzedAtUtc).ToLocalTime().Date)
            .ToDictionary(g => g.Key, g => g.Sum(m => (long)m.ChestsOpened));
        var days = Enumerable.Range(0, 30).Select(i => now.Date.AddDays(i - 29)).ToList();
        var max = Math.Max(1L, days.Select(d => daily.GetValueOrDefault(d)).Max());
        var bars = days.Select(d => new ReportDay(d, daily.GetValueOrDefault(d),
            Math.Max(3, 120d * daily.GetValueOrDefault(d) / max))).ToList();
        var bestDay = bars.MaxBy(d => d.Count)!;
        var other = matches.SelectMany(m => m.OtherInteractions.Count > 0
                ? m.OtherInteractions
                : new Dictionary<string, int>
                {
                    [ChestKinds.AmmoBox] = m.AmmoBoxesOpened,
                    [ChestKinds.OtherContainer] = m.OtherContainersOpened
                })
            .GroupBy(kv => kv.Key).Select(g => new ReportCount(L.Name(g.Key),
                g.Sum(kv => (long)kv.Value), "Fuera del total de cofres"))
            .Where(row => row.Count > 0).OrderByDescending(row => row.Count).ToList();
        var own = matches.SelectMany(m => m.TreasureFinds).ToList();
        var finds = matches.SelectMany(m =>
                m.TreasureFinds.Select(f => Find(m, f, false))
                    .Concat(m.TeamTreasureFinds.Select(f => Find(m, f, true))))
            .ToList();
        return new PlayerReport
        {
            Total = total,
            Session = matches.Where(session.Includes).Sum(m => (long)m.ChestsOpened),
            Matches = matches.Count,
            Average = matches.Count > 0 ? (double)total / matches.Count : 0,
            Best = matches.Select(m => m.ChestsOpened).DefaultIfEmpty(0).Max(),
            Treasures = own.Count,
            TeamTreasures = matches.Sum(m => m.TeamTreasureFinds.Count),
            Coverage = $"{chests.Count:N0} cofres con desglose de {total:N0} contabilizados. "
                + $"{chests.Count(c => c.Destroyed):N0} destruidos dentro del desglose. Las importaciones antiguas pueden conservar solo el total.",
            BestDay = bestDay.Count > 0
                ? $"Mejor día en los últimos 30 días: {bestDay.Date} · {bestDay.Count:N0} cofres"
                : "No hay aperturas en los últimos 30 días para esta cuenta y período.",
            Updated = $"Actualizado el {now:dd/MM/yyyy HH:mm:ss} · se renueva al terminar el análisis o cambiar cuenta y fechas.",
            Days = bars,
            Kinds = chests.GroupBy(c => ChestKinds.Canonical(c.Kind))
                .OrderByDescending(g => g.Count()).Select(g => new ReportCount(L.Name(g.Key), g.LongCount())).ToList(),
            Other = other,
            Sources = own.GroupBy(f => f.SourceLabel)
                .OrderByDescending(g => g.Count()).Select(g => new ReportCount(g.Key, g.LongCount(),
                    g.Any(f => !f.SourceIsChest) ? "Fiabilidad baja: origen distinto de un cofre"
                    : g.Any(f => !f.SourceCertain) ? "Incluye origen probable" : "Origen confirmado")).ToList(),
            Finds = finds,
            Latest = matches.Take(100).Select(m => new ReportMatch(
                When(m), Pretty.Playlist(m.Playlist), Pretty.Placement(m), m.ChestsOpened,
                string.Join(" · ", m.TreasureFinds.Select(f => f.DisplayName)))).ToList()
        };
    }

    private static string When(MatchRecord match) =>
        (match.MatchStartUtc ?? match.AnalyzedAtUtc).ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    private static ReportTreasure Find(MatchRecord match, TreasureFind find, bool team) => new(
        When(match), find.DisplayName,
        find.ObtainedAtSeconds is float seconds
            ? (find.TimeExact ? "" : "Hacia ") + Pretty.Clock(seconds) : "Desconocido",
        team && string.IsNullOrEmpty(find.Teammate) ? "Cofre fuera de tu vista (estimación)"
            : find.SourceLabel + (!find.SourceIsChest ? " · fiabilidad baja" : !find.SourceCertain ? " · probable" : ""),
        team ? (string.IsNullOrEmpty(find.Teammate) ? "Compañero" : find.Teammate) + " (estimación)" : "Tú", team);
}
