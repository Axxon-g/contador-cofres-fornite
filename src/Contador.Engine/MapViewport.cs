namespace Contador.Engine;

/// <summary>All input/output uses source-image pixels; no world-coordinate calibration.</summary>
public sealed class MapViewport
{
    public double Width { get; private set; } = 600;
    public double Height { get; private set; } = 600;
    public double Zoom { get; private set; } = 1;
    public double CenterX { get; private set; } = 1078;
    public double CenterY { get; private set; } = 1139.5;
    public double Scale => Math.Max(.001, Math.Min(Width / 1178, Height / 1177)) * Zoom;

    public void Resize(double width, double height)
    {
        Width = Math.Max(1, width);
        Height = Math.Max(1, height);
        Clamp();
    }

    public void Fit()
    {
        Zoom = 1;
        CenterX = 1078;
        CenterY = 1139.5;
    }

    public MapPosition ToScreen(double x, double y) => new(
        (x - CenterX) * Scale + Width / 2, (y - CenterY) * Scale + Height / 2);

    public MapPosition ToMap(double x, double y) => new(
        (x - Width / 2) / Scale + CenterX, (y - Height / 2) / Scale + CenterY);

    public void ZoomAt(double factor, double x, double y)
    {
        if (!double.IsFinite(factor) || factor <= 0) return;
        var anchor = ToMap(x, y);
        Zoom = Math.Clamp(Zoom * factor, 1, 8);
        CenterX = anchor.X - (x - Width / 2) / Scale;
        CenterY = anchor.Y - (y - Height / 2) / Scale;
        Clamp();
    }

    public void Focus(double x, double y, double zoom)
    {
        if (!MapCatalog.ValidPosition(x, y) || !double.IsFinite(zoom)) return;
        CenterX = x;
        CenterY = y;
        Zoom = Math.Clamp(zoom, 1, 8);
        Clamp();
    }

    public void Pan(double screenX, double screenY)
    {
        CenterX -= screenX / Scale;
        CenterY -= screenY / Scale;
        Clamp();
    }

    private void Clamp()
    {
        var halfX = Math.Min(589, Width / (2 * Scale));
        var halfY = Math.Min(588.5, Height / (2 * Scale));
        CenterX = Math.Clamp(CenterX, 489 + halfX, 1667 - halfX);
        CenterY = Math.Clamp(CenterY, 551 + halfY, 1728 - halfY);
    }
}

public sealed record MapCluster(double ScreenX, double ScreenY, IReadOnlyList<MapPoint> Points);

public static class MapClustering
{
    public static IReadOnlyList<MapCluster> Build(IEnumerable<MapPoint> points, MapViewport viewport)
    {
        var cell = viewport.Zoom >= 7.9 ? 16 : 32;
        var clusters = points.Select(point => (Point: point, Screen: viewport.ToScreen(point.X, point.Y)))
            .Where(p => p.Screen.X >= -24 && p.Screen.X <= viewport.Width + 24
                && p.Screen.Y >= -24 && p.Screen.Y <= viewport.Height + 24)
            .GroupBy(p => ((int)Math.Floor(p.Screen.X / cell), (int)Math.Floor(p.Screen.Y / cell)))
            .Select(group => new MapCluster(group.Average(p => p.Screen.X), group.Average(p => p.Screen.Y),
                group.Select(p => p.Point).ToArray())).ToList();

        // Merge across cell boundaries as well: the same final centers drive drawing and hit testing.
        bool merged;
        do
        {
            merged = false;
            for (var i = 0; i < clusters.Count; i++)
            {
                for (var j = i + 1; j < clusters.Count; j++)
                {
                    var a = clusters[i];
                    var b = clusters[j];
                    var spacing = (a.Points.Count > 1 ? 14 : 10)
                        + (b.Points.Count > 1 ? 14 : 10) + 3;
                    if (Math.Pow(a.ScreenX - b.ScreenX, 2)
                        + Math.Pow(a.ScreenY - b.ScreenY, 2) >= spacing * spacing) continue;
                    var count = a.Points.Count + b.Points.Count;
                    clusters[i] = new MapCluster(
                        (a.ScreenX * a.Points.Count + b.ScreenX * b.Points.Count) / count,
                        (a.ScreenY * a.Points.Count + b.ScreenY * b.Points.Count) / count,
                        a.Points.Concat(b.Points).ToArray());
                    clusters.RemoveAt(j);
                    merged = true;
                    break;
                }
                if (merged) break;
            }
        } while (merged);
        return clusters;
    }
}
