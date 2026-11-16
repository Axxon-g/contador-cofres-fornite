namespace ChestCounter;

/// <summary>Bounded retries for temporary failures; invalid replay contents need an explicit retry.</summary>
public sealed class ReplayRetryPolicy
{
    private readonly Dictionary<string, (int Failures, DateTime Due)> pending = new(StringComparer.Ordinal);

    public bool IsDue(string key, DateTime utcNow) => !pending.TryGetValue(key, out var entry) || utcNow >= entry.Due;
    public bool HasPending => pending.Count > 0;

    public bool Schedule(string key, Exception error, DateTime utcNow)
    {
        var failures = pending.TryGetValue(key, out var entry) ? entry.Failures + 1 : 1;
        var temporary = error is IOException
            or UnauthorizedAccessException or TimeoutException;
        if (!temporary || failures >= 3)
        {
            pending.Remove(key);
            return false;
        }

        pending[key] = (failures, utcNow.AddMinutes(2 * failures));
        return true;
    }

    public void Complete(string key) => pending.Remove(key);
    public void Reset() => pending.Clear();
    public void Retain(IReadOnlySet<string> keys)
    {
        foreach (var key in pending.Keys.Where(key => !keys.Contains(key)).ToArray()) pending.Remove(key);
    }
}
