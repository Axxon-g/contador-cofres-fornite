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

    public static TeamAccounting Create(IEnumerable<MatchRecord> source)
    {
        var matches = source.ToList();
        var covered = matches.Where(m => m.TeammateChestsOpened is >= 0).ToList();
        var own = matches.SelectMany(m => m.TreasureFinds).ToList();
        var team = matches.SelectMany(m => m.TeamTreasureFinds).ToList();
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
            TeammateChests = covered.Sum(m => (long)m.TeammateChestsOpened!.Value),
            OwnChestRewards = own.Count(f => FromChest(f) && DirectOrigin(f)),
            OwnProbableChestRewards = own.Count(f => FromChest(f) && !DirectOrigin(f)),
            OwnOtherRewards = own.Count(f => !FromChest(f)),
            TeammateRewards = team.Count(f => DirectOrigin(f) && !string.IsNullOrEmpty(f.Teammate)),
            EstimatedTeammateRewards = team.Count(f => !DirectOrigin(f) || string.IsNullOrEmpty(f.Teammate))
        };
    }
}
