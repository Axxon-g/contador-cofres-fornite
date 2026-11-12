namespace ChestCounter;

/// <summary>Requires two identical snapshots and a short quiet period before decoding a finalized replay.</summary>
public sealed class ReplayFileStability
{
    public static bool HasChanged(FileInfo file,string expectedKey)
    {
        try { file.Refresh(); return !file.Exists || Tracker.FileKey(file)!=expectedKey; }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException) { return true; }
    }
    private readonly Dictionary<string,(long Length,DateTime Written,DateTime Observed)> snapshots = new(StringComparer.OrdinalIgnoreCase);
    public bool Observe(string path,long length,DateTime writtenUtc,DateTime nowUtc,TimeSpan quietPeriod)
    {
        var stable = snapshots.TryGetValue(path,out var previous)
            && previous.Length == length && previous.Written == writtenUtc
            && nowUtc-previous.Observed >= quietPeriod && nowUtc-writtenUtc >= quietPeriod;
        if (!snapshots.TryGetValue(path,out previous) || previous.Length != length || previous.Written != writtenUtc)
            snapshots[path]=(length,writtenUtc,nowUtc);
        return stable;
    }
}
