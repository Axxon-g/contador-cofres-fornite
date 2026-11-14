namespace Unreal.Core;

/// <summary>Allocation limits shared by replay entry points, including sparse capacities.</summary>
public static class ReplayLimits
{
    public const int MaxInputBytes = 512 * 1024 * 1024;
    public const int MaxChunkBytes = 64 * 1024 * 1024;
    public const int MaxElements = 262_144;
    private const long MetadataBudget = 256L * 1024 * 1024;
    [ThreadStatic] private static Budget? current;

    private sealed class Budget { public long Remaining = MetadataBudget; }

    public static IDisposable Begin()
    {
        var previous = current;
        current = new Budget();
        return new Scope(previous);
    }

    private sealed class Scope(Budget? previous) : IDisposable
    {
        public void Dispose() => current = previous;
    }

    public static int Reserve(uint count, int elementBytes = 64)
    {
        if (count > MaxElements || elementBytes < 1)
            throw new InvalidDataException("Replay collection exceeds the element limit.");
        var bytes = checked((long)count * elementBytes);
        var budget = current ??= new Budget();
        if (bytes > budget.Remaining)
            throw new InvalidDataException("Replay exceeds the metadata allocation budget.");
        budget.Remaining -= bytes;
        return (int)count;
    }

    public static byte[] ReadInput(Stream stream)
    {
        if (stream.CanSeek)
        {
            var length = stream.Length - stream.Position;
            if (length < 0 || length > MaxInputBytes)
                throw new InvalidDataException("Replay exceeds the 512 MiB input limit.");
            var bytes = new byte[(int)length];
            stream.ReadExactly(bytes);
            return bytes;
        }
        using var output = new MemoryStream();
        var block = new byte[8192];
        int read;
        while ((read = stream.Read(block)) > 0)
        {
            if (output.Length + read > MaxInputBytes)
                throw new InvalidDataException("Replay exceeds the 512 MiB input limit.");
            output.Write(block, 0, read);
        }
        return output.ToArray();
    }
}
