using ChestCounter;

namespace Contador.Engine;

/// <summary>Observed team figures remain separate from personal totals and unknown legacy data.</summary>
public sealed record TeamAccounting
{
    public int Matches { get; init; }
    public int CoveredMatches { get; init; }
    public long OwnCoveredChests { get; init; }
    public long OwnChests { get; init; }
    public long TeammateChests { get; init; }
    public int ReplaySourcesUsed { get; init; }
    public int ConflictingSources { get; init; }
    public int UnattributedInteractions { get; init; }
    public string MemberSummary { get; init; } = "";
    public long TeamChests => OwnChests + TeammateChests;
    public int OwnRecordedRewards => OwnChestRewards + OwnProbableChestRewards + OwnOtherRewards;
    public int OwnChestRewards { get; init; }
    public int OwnProbableChestRewards { get; init; }
    public int OwnOtherRewards { get; init; }
    public int TeammateRewards { get; init; }
    public int EstimatedTeammateRewards { get; init; }

    public static long? LastTeamTotal(MatchRecord? match) =>
        match?.TeammateChestsOpened is >= 0
            ? (long)match.ChestsOpened + match.TeammateChestsOpened.Value : null;

    public static TeamAccounting Create(IEnumerable<MatchRecord> source, IEnumerable<MatchRecord>? additionalSources = null)
    {
        var matches = source.ToList();
        var covered = matches.Where(m => m.TeammateChestsOpened is >= 0).ToList();
        var own = matches.SelectMany(m => m.TreasureFinds).ToList();
        var team = matches.SelectMany(m => m.TeamTreasureFinds).ToList();
        var sources = additionalSources?.ToList() ?? new List<MatchRecord>();
        var reconciled = covered.Select(match => TeamReplayReconciliation.Resolve(match, sources)).ToList();
        static bool FromChest(TreasureFind find) =>
            find.SourceIsChest && ChestKinds.IsChest(find.SourceKind);
        // SourceCertain in old records also describes nearest-interaction heuristics.
        // Only a golden-chest event is direct evidence of origin, independent of that flag.
        static bool DirectOrigin(TreasureFind find) => find.OriginDirectlyObserved;
        return new TeamAccounting
        {
            Matches = matches.Count,
            CoveredMatches = covered.Count,
            OwnChests = matches.Sum(m => (long)m.ChestsOpened),
            OwnCoveredChests = covered.Sum(m => (long)m.ChestsOpened),
            TeammateChests = reconciled.Sum(result => result.TeammateChests),
            ReplaySourcesUsed = reconciled.Sum(result => result.Sources),
            ConflictingSources = reconciled.Sum(result => result.Conflicts),
            UnattributedInteractions = matches.Sum(match => match.UnattributedChestInteractions),
            MemberSummary = string.Join(" · ", reconciled.SelectMany(result => result.Members)
                .GroupBy(member => member.EpicId ?? "name:" + member.Name, StringComparer.OrdinalIgnoreCase)
                .Select(group => $"{group.Last().Name}: {group.Sum(member => (long)member.Chests):N0} registrados")),
            OwnChestRewards = own.Count(f => FromChest(f) && DirectOrigin(f)),
            OwnProbableChestRewards = own.Count(f => FromChest(f) && !DirectOrigin(f)),
            OwnOtherRewards = own.Count(f => !FromChest(f)),
            TeammateRewards = team.Count(f => DirectOrigin(f) && !string.IsNullOrEmpty(f.Teammate)),
            EstimatedTeammateRewards = team.Count(f => !DirectOrigin(f) || string.IsNullOrEmpty(f.Teammate))
        };
    }
}
