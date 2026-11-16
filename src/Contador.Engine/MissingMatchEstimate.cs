namespace Contador.Engine;

/// <summary>A scenario, not recovered history. The caller must supply comparable account/period/modes.</summary>
public static class MissingMatchEstimate
{
    public sealed record Result(long MissingMatches, double Mean, double EstimatedChests);

    public static Result Calculate(long played, int recordedMatches, long recordedChests)
    {
        if (played < 0 || played > 10_000_000 || recordedMatches <= 0 || recordedChests < 0)
            throw new ArgumentOutOfRangeException(nameof(played));
        var missing = Math.Max(0, played - recordedMatches);
        var mean = (double)recordedChests / recordedMatches;
        return new Result(missing, mean, missing * mean);
    }
}
