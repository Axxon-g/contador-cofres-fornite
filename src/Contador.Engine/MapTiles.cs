namespace Contador.Engine;

public readonly record struct MapTile(int Level, int X, int Y)
{
    public const int PixelSize = 256;
    public double MapSize => 2048d / (1 << Level);
    public double MapX => X * MapSize;
    public double MapY => Y * MapSize;
    public bool IsValid => Level is >= 3 and <= 7
        && X >= 0 && Y >= 0 && X < (1 << Level) && Y < (1 << Level);
    public Uri Url => IsValid
        ? new Uri($"https://fortnite.gg/maps/43/{Level}/{X}/{Y}.webp")
        : throw new ArgumentOutOfRangeException(nameof(Level));
    public string FileName => $"43-{Level}-{X}-{Y}.webp";
}

public static class MapTiles
{
    // Both images use a 2048-unit square. Resolution changes, not marker coordinates.
    public static IReadOnlyList<MapTile> Visible(MapViewport viewport, double dpiScale = 1)
    {
        if (!double.IsFinite(dpiScale) || dpiScale <= 0) dpiScale = 1;
        var level = Math.Clamp((int)Math.Ceiling(Math.Log2(viewport.Scale * dpiScale * 8)), 3, 7);
        var count = 1 << level;
        var size = 2048d / count;
        var topLeft = viewport.ToMap(0, 0);
        var bottomRight = viewport.ToMap(viewport.Width, viewport.Height);
        var firstX = Math.Clamp((int)Math.Floor(topLeft.X / size), 0, count - 1);
        var lastX = Math.Clamp((int)Math.Floor(bottomRight.X / size), 0, count - 1);
        var firstY = Math.Clamp((int)Math.Floor(topLeft.Y / size), 0, count - 1);
        var lastY = Math.Clamp((int)Math.Floor(bottomRight.Y / size), 0, count - 1);
        var result = new List<MapTile>();
        for (var y = firstY; y <= lastY; y++)
        for (var x = firstX; x <= lastX; x++) result.Add(new(level, x, y));
        return result;
    }
}
