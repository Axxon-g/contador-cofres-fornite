using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Contador.Engine;

namespace Contador;

public sealed class InteractiveMap : FrameworkElement
{
    private static readonly Lazy<BitmapSource> Island = new(LoadImage);
    private static readonly Brush Ground = Brush("#101C2E");
    private static readonly Brush Ink = Brush("#FFF4DF");
    private static readonly Brush RouteBrush = Brush("#FF9A8B");
    private static readonly IReadOnlyDictionary<string, Brush> Colors = MapCatalog.Categories
        .ToDictionary(c => c.Key, c => Brush(c.Color));
    public MapViewport Viewport { get; } = new();
    public event EventHandler? ViewChanged;
    private Point start, last;
    private bool dragging;
    private int dragStop = -1;
    private MapPosition? stopPreview;
    private MapTileCache? tileCache;
    public string? TileCacheFolder { get; set; }
    public bool TileNetworkAllowed { get; set; } = true;
    public static readonly DependencyProperty DetailStatusProperty = DependencyProperty.Register(
        nameof(DetailStatus), typeof(string), typeof(InteractiveMap),
        new PropertyMetadata("Mapa integrado · activa Terreno HD para cargar más detalle por Internet."));
    public string DetailStatus
    {
        get => (string)GetValue(DetailStatusProperty);
        private set => SetValue(DetailStatusProperty, value);
    }

    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(
        nameof(Model), typeof(MapPlannerModel), typeof(InteractiveMap),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, ModelChanged));
    public MapPlannerModel? Model
    {
        get => (MapPlannerModel?)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    public InteractiveMap()
    {
        Focusable = true;
        ClipToBounds = true;
        Cursor = Cursors.Hand;
        SizeChanged += (_, _) => { Viewport.Resize(ActualWidth, ActualHeight); RefreshView(); };
        Unloaded += (_, _) => { tileCache?.Dispose(); tileCache = null; };
    }

    private static Brush Brush(string value)
    {
        var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(value)!;
        brush.Freeze();
        return brush;
    }

    private static BitmapSource LoadImage()
    {
        using var stream = typeof(InteractiveMap).Assembly.GetManifestResourceStream("Contador.Map.Island4310")
            ?? throw new InvalidDataException("Falta el mapa integrado.");
        using var reader = new StreamReader(stream);
        using var bytes = new MemoryStream(Convert.FromBase64String(reader.ReadToEnd()));
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = bytes;
        image.EndInit();
        image.Freeze();
        if (image.PixelWidth != 2048 || image.PixelHeight != 2048)
            throw new InvalidDataException("El mapa no coincide con el catálogo de puntos.");
        return image;
    }

    private static void ModelChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        var map = (InteractiveMap)target;
        if (e.OldValue is MapPlannerModel previous)
            PropertyChangedEventManager.RemoveHandler(previous, map.ModelUpdated, "");
        if (e.NewValue is MapPlannerModel current)
            PropertyChangedEventManager.AddHandler(current, map.ModelUpdated, "");
    }
    private void ModelUpdated(object? sender, PropertyChangedEventArgs e)
    {
        Cursor = Model?.EditRoute == true ? Cursors.Cross : Cursors.Hand;
        if (Model?.HighDetailEnabled != true)
        {
            tileCache?.Dispose();
            tileCache = null;
            DetailStatus = "Mapa integrado · HD desactivado · sin nuevas descargas.";
        }
        InvalidateVisual();
    }
    public void Fit() { Viewport.Fit(); RefreshView(); }
    public void Zoom(double factor) { Viewport.ZoomAt(factor, ActualWidth / 2, ActualHeight / 2); RefreshView(); }
    public void FocusPoint(double x, double y, double zoom = 4) { Viewport.Focus(x, y, zoom); RefreshView(); }
    private void RefreshView() { InvalidateVisual(); ViewChanged?.Invoke(this, EventArgs.Empty); }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (ActualWidth < 20 || ActualHeight < 20 || Model is not { } model) return;
        dc.DrawRectangle(Ground, null, new Rect(RenderSize));
        var origin = Viewport.ToScreen(0, 0);
        dc.DrawImage(Island.Value, new Rect(origin.X, origin.Y, 2048 * Viewport.Scale, 2048 * Viewport.Scale));
        if (model.HighDetailEnabled) DrawDetail(dc);
        foreach (var place in model.FilteredPoints.Where(p => p.Kind == "place"))
        {
            var pos = Viewport.ToScreen(place.X, place.Y);
            if (pos.X < -80 || pos.X > ActualWidth + 80 || pos.Y < -30 || pos.Y > ActualHeight + 30) continue;
            var text = Text(place.Name, 12, Ink);
            var geometry = text.BuildGeometry(new Point(pos.X - text.Width / 2, pos.Y - 28));
            dc.DrawGeometry(Ground, new Pen(Ground, 3), geometry);
            dc.DrawGeometry(Ink, null, geometry);
        }
        foreach (var cluster in Clusters())
        {
            var center = new Point(cluster.ScreenX, cluster.ScreenY);
            var first = cluster.Points[0];
            var color = Colors[first.Kind];
            var radius = cluster.Points.Count > 1 ? 14 : 10;
            dc.DrawEllipse(Ground, new Pen(color, 2), center, radius, radius);
            if (cluster.Points.Count > 1)
            {
                var number = Text(cluster.Points.Count.ToString(), 12, Ink);
                dc.DrawText(number, new Point(center.X - number.Width / 2, center.Y - number.Height / 2));
            }
            else DrawIcon(dc, first.Kind, center, color);
            if (cluster.Points.All(point => model.Plan.State.Reviews.ContainsKey(point.Id)))
            {
                dc.DrawEllipse(Ground, new Pen(Ink, 1), new Point(center.X + 9, center.Y + 9), 5, 5);
                dc.DrawLine(new Pen(Ink, 1.5), new Point(center.X + 6, center.Y + 9), new Point(center.X + 8, center.Y + 11));
                dc.DrawLine(new Pen(Ink, 1.5), new Point(center.X + 8, center.Y + 11), new Point(center.X + 12, center.Y + 7));
            }
            if (model.SelectedPoint != null && cluster.Points.Contains(model.SelectedPoint))
                dc.DrawEllipse(null, new Pen(Ink, 2), center, radius + 4, radius + 4);
        }
        if (model.ShowRoute)
        {
            var nodes = model.Plan.State.Route.Select((node, i) =>
            {
                var position = i == dragStop && stopPreview != null ? stopPreview : new MapPosition(node.X, node.Y);
                var screen = Viewport.ToScreen(position.X, position.Y);
                return new Point(screen.X, screen.Y);
            }).ToList();
            for (var i = 1; i < nodes.Count; i++)
            {
                dc.DrawLine(new Pen(Ground, 5), nodes[i - 1], nodes[i]);
                dc.DrawLine(new Pen(RouteBrush, 2) { DashStyle = DashStyles.Dash }, nodes[i - 1], nodes[i]);
            }
            for (var i = 0; i < nodes.Count; i++)
            {
                dc.DrawEllipse(Ground, new Pen(RouteBrush, 2), nodes[i], 11, 11);
                var number = Text((i + 1).ToString(), 12, Ink);
                dc.DrawText(number, new Point(nodes[i].X - number.Width / 2, nodes[i].Y - number.Height / 2));
            }
        }
        if (IsKeyboardFocused)
            dc.DrawRectangle(null, new Pen(Ink, 2), new Rect(1, 1, Math.Max(0, ActualWidth - 2), Math.Max(0, ActualHeight - 2)));
    }

    private void DrawDetail(DrawingContext dc)
    {
        tileCache ??= new MapTileCache(() =>
        {
            if (!Dispatcher.HasShutdownStarted)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (tileCache == null) return;
                    DetailStatus = tileCache.Status;
                    InvalidateVisual();
                }));
        }, TileCacheFolder, TileNetworkAllowed);
        var visible = MapTiles.Visible(Viewport, VisualTreeHelper.GetDpi(this).DpiScaleX);
        var wanted = visible.Concat(visible.SelectMany(tile => Enumerable.Range(3, tile.Level - 3)
            .Select(level => new MapTile(level, tile.X >> (tile.Level - level), tile.Y >> (tile.Level - level)))))
            .Distinct().OrderBy(tile => tile.Level).ToArray();
        tileCache.Request(wanted);
        DetailStatus = tileCache.Status;
        foreach (var tile in wanted)
        {
            if (tileCache.Get(tile) is not { } image) continue;
            var topLeft = Viewport.ToScreen(tile.MapX, tile.MapY);
            var size = tile.MapSize * Viewport.Scale;
            dc.DrawImage(image, new Rect(topLeft.X, topLeft.Y, size + .25, size + .25));
        }
    }

    private FormattedText Text(string value, double size, Brush color) => new(value,
        CultureInfo.GetCultureInfo("es-MX"), FlowDirection.LeftToRight, new Typeface("Segoe UI"), size,
        color, VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private static void DrawIcon(DrawingContext dc, string kind, Point p, Brush color)
    {
        var pen = new Pen(color, 1.5);
        if (kind is "chest" or "rare_chest")
        {
            dc.DrawRectangle(null, pen, new Rect(p.X - 5, p.Y - 4, 10, 8));
            dc.DrawLine(pen, new Point(p.X - 5, p.Y - 1), new Point(p.X + 5, p.Y - 1));
            dc.DrawRectangle(color, null, new Rect(p.X - 1, p.Y - 2, 2, 4));
        }
        else if (kind == "buried_chest")
        {
            dc.DrawLine(pen, new Point(p.X - 4, p.Y - 5), new Point(p.X + 4, p.Y + 4));
            dc.DrawRectangle(null, pen, new Rect(p.X - 5, p.Y - 5, 4, 3));
            dc.DrawLine(pen, new Point(p.X + 1, p.Y + 5), new Point(p.X + 5, p.Y + 1));
        }
        else if (kind == "vault")
        {
            dc.DrawRectangle(null, pen, new Rect(p.X - 5, p.Y - 5, 10, 10));
            dc.DrawEllipse(null, pen, p, 3, 3);
        }
        else if (kind == "loot_cannon")
        {
            dc.DrawEllipse(null, pen, new Point(p.X - 3, p.Y + 3), 2, 2);
            dc.DrawLine(new Pen(color, 4), new Point(p.X - 3, p.Y + 1), new Point(p.X + 4, p.Y - 3));
        }
        else if (kind == "sprite_shrine")
        {
            dc.DrawLine(pen, new Point(p.X, p.Y - 6), new Point(p.X + 5, p.Y));
            dc.DrawLine(pen, new Point(p.X + 5, p.Y), new Point(p.X, p.Y + 6));
            dc.DrawLine(pen, new Point(p.X, p.Y + 6), new Point(p.X - 5, p.Y));
            dc.DrawLine(pen, new Point(p.X - 5, p.Y), new Point(p.X, p.Y - 6));
        }
        else
        {
            dc.DrawRectangle(null, pen, new Rect(p.X - 4, p.Y - 5, 8, 10));
            dc.DrawLine(pen, new Point(p.X - 2, p.Y - 1), new Point(p.X + 2, p.Y - 1));
            dc.DrawLine(pen, new Point(p.X - 2, p.Y + 2), new Point(p.X + 2, p.Y + 2));
        }
    }

    private IReadOnlyList<MapCluster> Clusters() => MapClustering.Build(
        Model?.FilteredPoints.Where(p => p.Kind != "place") ?? [], Viewport);

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        var p = e.GetPosition(this);
        Viewport.ZoomAt(e.Delta > 0 ? 1.3 : 1 / 1.3, p.X, p.Y);
        RefreshView();
        e.Handled = true;
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        Focus();
        start = last = e.GetPosition(this);
        dragging = false;
        dragStop = -1;
        if (Model is { EditRoute: true, ShowRoute: true } model)
        {
            dragStop = model.Plan.State.Route.FindIndex(node =>
            {
                var point = Viewport.ToScreen(node.X, node.Y);
                return Distance(start.X, start.Y, point.X, point.Y) < 14;
            });
        }
        CaptureMouse();
        e.Handled = true;
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        var point = e.GetPosition(this);
        if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
        {
            dragging |= Distance(start.X, start.Y, point.X, point.Y) > 4;
            if (dragging)
            {
                if (dragStop >= 0) stopPreview = Viewport.ToMap(point.X, point.Y);
                else Viewport.Pan(point.X - last.X, point.Y - last.Y);
                RefreshView();
            }
            last = point;
            return;
        }
        var hit = Hit(point);
        ToolTip = hit == null ? "Rueda: zoom. Arrastra para mover. En edición, pulsa para añadir una parada." :
            hit.Points.Count > 1 ? $"{hit.Points.Count} puntos próximos. Pulsa para acercarte o elegir uno." : hit.Points[0].ListLabel;
    }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!IsMouseCaptured) return;
        var point = e.GetPosition(this);
        if (dragging && dragStop >= 0 && stopPreview != null)
            Model?.MoveStop(dragStop, stopPreview.X, stopPreview.Y);
        else if (!dragging)
        {
            if (Model?.EditRoute == true && dragStop < 0)
            {
                var position = Viewport.ToMap(point.X, point.Y);
                Model.AddStop(position.X, position.Y);
            }
            else if (Hit(point) is { } group)
            {
                Model?.SelectGroup(group.Points);
                if (group.Points.Count > 1 && Viewport.Zoom < MapViewport.MaxZoom)
                    Viewport.Focus(group.Points.Average(p => p.X), group.Points.Average(p => p.Y),
                        Math.Min(MapViewport.MaxZoom, Viewport.Zoom * 2));
                else if (group.Points[0].Kind == "place")
                    Viewport.Focus(group.Points[0].X, group.Points[0].Y, Math.Max(4, Viewport.Zoom));
            }
        }
        dragStop = -1;
        stopPreview = null;
        ReleaseMouseCapture();
        RefreshView();
        e.Handled = true;
    }
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        dragStop = -1;
        stopPreview = null;
        InvalidateVisual();
        base.OnLostMouseCapture(e);
    }
    private MapCluster? Hit(Point point)
    {
        var cluster = Clusters().OrderBy(c => Distance(point.X, point.Y, c.ScreenX, c.ScreenY))
            .FirstOrDefault(c => Distance(point.X, point.Y, c.ScreenX, c.ScreenY) <= 18);
        if (cluster != null || Model == null) return cluster;
        foreach (var place in Model.FilteredPoints.Where(p => p.Kind == "place"))
        {
            var screen = Viewport.ToScreen(place.X, place.Y);
            var label = Text(place.Name, 12, Ink);
            if (new Rect(screen.X - label.Width / 2, screen.Y - 28, label.Width, label.Height).Contains(point))
                return new MapCluster(screen.X, screen.Y, [place]);
        }
        return null;
    }
    private static double Distance(double x, double y, double x2, double y2) => Math.Sqrt(Math.Pow(x - x2, 2) + Math.Pow(y - y2, 2));

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var handled = true;
        switch (e.Key)
        {
            case Key.Add: case Key.OemPlus: Zoom(1.3); break;
            case Key.Subtract: case Key.OemMinus: Zoom(1 / 1.3); break;
            case Key.Home: Fit(); break;
            case Key.Left: Viewport.Pan(48, 0); break;
            case Key.Right: Viewport.Pan(-48, 0); break;
            case Key.Up: Viewport.Pan(0, 48); break;
            case Key.Down: Viewport.Pan(0, -48); break;
            case Key.Z when Keyboard.Modifiers.HasFlag(ModifierKeys.Control): Model?.Undo(); break;
            case Key.Escape: if (Model != null) Model.EditRoute = false; break;
            default: handled = false; break;
        }
        if (handled) { RefreshView(); e.Handled = true; }
        else base.OnKeyDown(e);
    }
}
