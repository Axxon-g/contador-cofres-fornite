using ChestCounter;

namespace Contador.Engine;

/// <summary>One contribution per teammate, using the strongest verified local perspective.</summary>
public static class TeamReplayReconciliation
{
    public sealed record Member(string? EpicId, string Name, int Observed, int Chests,
        bool OwnPerspective, bool Supplemented, bool Conflict, string? SourceOwner, string? SourceFile);

    public sealed record Result(long TeammateChests, int Sources, int Conflicts)
    {
        public IReadOnlyList<Member> Members { get; init; } = Array.Empty<Member>();
    }

    public static bool SameSession(string left, string right) =>
        Guid.TryParse(left, out var a) && Guid.TryParse(right, out var b)
            ? a == b : string.Equals(left, right, StringComparison.Ordinal);

    public static bool CanUse(MatchRecord match, MatchRecord source) =>
        match.SessionIdentityVerified && source.SessionIdentityVerified
        && source.IsReplayOwner && source.AnalyzerVersion >= ReplayAnalyzer.CurrentVersion
        && source.NotTrackedOwner == null && source.ChestsOpened >= 0
        && !string.IsNullOrWhiteSpace(source.EpicId) && !string.IsNullOrWhiteSpace(match.SessionId)
        && SameSession(match.SessionId, source.SessionId)
        && !string.Equals(match.EpicId, source.EpicId, StringComparison.OrdinalIgnoreCase)
        && match.TeamChests.Any(member => string.Equals(member.EpicId, source.EpicId, StringComparison.OrdinalIgnoreCase));

    public static Result Resolve(MatchRecord match, IEnumerable<MatchRecord> sources)
    {
        var observed = Math.Max(0, match.TeammateChestsOpened ?? 0);
        var candidates = sources.Where(source => CanUse(match, source)).ToList();
        var members = new List<Member>();
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var roster = match.TeamChests.Select((member, index) => (member, index))
            .Where(entry => string.IsNullOrWhiteSpace(entry.member.EpicId)
                || !string.Equals(entry.member.EpicId, match.EpicId, StringComparison.OrdinalIgnoreCase))
            .GroupBy(entry => string.IsNullOrWhiteSpace(entry.member.EpicId)
                ? "unknown:" + entry.index : "id:" + entry.member.EpicId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.MaxBy(entry => entry.member.Chests).member);

        foreach (var teammate in roster)
        {
            var minimum = Math.Max(0, teammate.Chests);
            var best = minimum;
            MatchRecord? winner = null;
            var ownPerspective = false;
            var conflict = false;
            if (!string.IsNullOrWhiteSpace(teammate.EpicId))
            {
                foreach (var source in candidates)
                {
                    var isOwn = string.Equals(source.EpicId, teammate.EpicId, StringComparison.OrdinalIgnoreCase);
                    var value = isOwn ? source.ChestsOpened : source.TeamChests
                        .Where(member => string.Equals(member.EpicId, teammate.EpicId, StringComparison.OrdinalIgnoreCase))
                        .Select(member => member.Chests).DefaultIfEmpty(-1).Max();
                    if (isOwn && value < minimum) conflict = true;
                    // Never add two perspectives together. A peer observation remains a minimum.
                    if (value > best || (value == best && isOwn && !ownPerspective))
                    {
                        best = value;
                        winner = source;
                        ownPerspective = isOwn;
                    }
                }
            }
            if (winner != null) used.Add(winner.EpicId!);
            members.Add(new Member(teammate.EpicId, teammate.PlayerName, minimum, best,
                ownPerspective, winner != null, conflict && !ownPerspective,
                winner?.PlayerName, winner?.ReplayFile));
        }
        // Preserve legacy/unattributed contributions outside the identified roster.
        var residual = Math.Max(0L, observed - members.Sum(member => (long)member.Observed));
        return new Result(residual + members.Sum(member => (long)member.Chests), used.Count,
            members.Count(member => member.Conflict))
        { Members = members };
    }

    public static MatchRecord ReadSource(string path)
    {
        var file = new FileInfo(path);
        if (!file.Exists || file.Length > Unreal.Core.ReplayLimits.MaxInputBytes)
            throw new InvalidDataException("La repetición no existe o supera el límite admitido.");
        if (ReplayHeaderProbe.IsLive(path) != false || ReplayHeaderProbe.KeyMissing(path))
            throw new InvalidDataException("La repetición del compañero debe estar finalizada y tener su clave.");
        using var input = file.OpenRead();
        var result = new ReplayAnalyzer().Analyze(input, file.Name, "");
        if (!result.IsReplayOwner || !result.SessionIdentityVerified || string.IsNullOrWhiteSpace(result.EpicId))
            throw new InvalidDataException("No se pudieron verificar la partida y el autor del archivo.");
        // Minimal summary includes teammate observations so another perspective can fill gaps.
        // Inventory, rewards and replay bytes are not persisted here.
        return new MatchRecord
        {
            SessionId = result.SessionId,
            SessionIdentityVerified = true,
            EpicId = result.EpicId,
            PlayerName = result.PlayerName,
            ReplayFile = file.Name,
            AnalyzedAtUtc = result.AnalyzedAtUtc,
            MatchStartUtc = result.MatchStartUtc,
            DurationSeconds = result.DurationSeconds,
            GameVersion = result.GameVersion,
            IsReplayOwner = true,
            AnalyzerVersion = result.AnalyzerVersion,
            ChestsOpened = result.ChestsOpened,
            TeammateChestsOpened = result.TeammateChestsOpened,
            TeamChests = result.TeamChests
        };
    }
}
