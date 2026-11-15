using ChestCounter;

namespace Contador.Engine;

/// <summary>Only probes recording headers; never decodes a match or reads the game process.</summary>
public static class MatchPresence
{
    public static bool IsRecording(string folder, DateTime nowUtc)
        => FindRecording(folder, nowUtc) != null;

    public static string? FindRecording(string folder, DateTime nowUtc)
    {
        if (!Directory.Exists(folder)) return null;
        try
        {
            return new DirectoryInfo(folder).EnumerateFiles("*.replay")
                .Where(f => nowUtc - f.LastWriteTimeUtc <= TimeSpan.FromMinutes(3))
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault(f => ReplayHeaderProbe.IsLive(f.FullName) == true)?.FullName;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return null; }
    }
}

/// <summary>Session is completed matches played since opening, not old replays imported at startup.</summary>
public sealed class SessionCounter(DateTime startedUtc)
{
    private readonly HashSet<string> baseline = new(StringComparer.Ordinal);
    public void ExcludeHistory(TrackerData data)
    {
        foreach (var match in data.Matches) baseline.Add(match.SessionId);
    }
    public bool Includes(MatchRecord m) =>
        !baseline.Contains(m.SessionId) &&
        (m.MatchStartUtc?.AddSeconds(m.DurationSeconds ?? 0) ?? m.AnalyzedAtUtc) >= startedUtc;
    public List<MatchRecord> Matches(TrackerData data) => data.Matches.Where(Includes).ToList();
    public OverlayView Overlay(TrackerData view, bool recording)
    {
        var matches = Matches(view);
        var count = matches.Sum(m => m.ChestsOpened);
        var model = OverlayModel.Build(view, null, recording);
        var last = view.Matches.OrderByDescending(m => m.MatchStartUtc ?? m.AnalyzedAtUtc).FirstOrDefault();
        var teamTotal = last?.TeammateChestsOpened is >= 0
            ? (long?)last.ChestsOpened + TeamReplayReconciliation.Resolve(last, view.TeamReplaySources).TeammateChests
            : null;
        return model with
        {
            LastTeamChests = teamTotal.HasValue ? "≥ " + teamTotal.Value.ToString("N0", L.Culture) : null,
            Session = L.N(count),
            Rows = model.Rows.Select(r => r.Label == "Sesión"
                ? r with { Value = L.N(count) + " cofres", Detail = matches.Count + " partidas desde que abriste la app" } : r).ToList()
        };
    }
}
