using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Contador.Engine;

namespace Contador;

public sealed class MapLayer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public string Key { get; }
    public string Color { get; }
    public string Caption { get; }
    private bool visible;
    public bool Visible
    {
        get => visible;
        set
        {
            if (visible == value) return;
            visible = value;
            PropertyChanged?.Invoke(this, new(nameof(Visible)));
        }
    }
    public MapLayer(MapCategory category, int count, bool enabled)
    {
        Key = category.Key;
        Color = category.Color;
        Caption = $"{category.Name} · {count:N0}";
        visible = enabled;
    }
}

public sealed record PlannerRouteRow(int Index, RouteNode Node)
{
    public string Label => $"{Index + 1}. {Node.Name}";
}

/// <summary>Local planning only: never changes the replay counter or its history.</summary>
public sealed class MapPlannerModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public MapCatalog Catalog { get; } = MapCatalog.Current;
    public MapPlan Plan { get; }
    public IReadOnlyList<MapLayer> Layers { get; }
    public IReadOnlyList<RouteGuide> Guides => RouteGuides.All;
    public IReadOnlyList<string> PlaceNames { get; }
    public GuideSource Official => RouteGuides.Official;
    public string Dataset => MapCatalog.Status;
    public string Source => MapCatalog.SourceUrl;
    public string Notice => "Bóvedas: nueve referencias aproximadas de una imagen aportada, no entradas verificadas. Las demás capas conservan datos comunitarios sin validar punto por punto. No hay datos de altura o planta ni detección del juego. Las marcas no suman cofres al contador.";
    private readonly string? statePath;
    private bool canSave = true;
    private string search = "";
    private bool hideReviewed, editRoute;
    private bool highDetailEnabled = true;
    private bool showRoute = true;
    private RouteGuide? selectedGuide;
    private MapPoint? selectedPoint;
    private PlannerRouteRow? selectedStop;
    public IReadOnlyList<MapPoint> FilteredPoints { get; private set; } = [];
    public IReadOnlyList<MapPoint> NearbyPoints { get; private set; } = [];
    public IReadOnlyList<PlannerRouteRow> RouteRows => Plan.State.Route.Select((n, i) => new PlannerRouteRow(i, n)).ToList();
    public string SaveMessage { get; private set; } = "Plan y marcas guardados solo en este equipo.";
    public string CountLabel => $"{FilteredPoints.Count:N0} puntos con estos filtros · {Plan.State.Reviews.Count:N0} revisados · {Plan.State.Route.Count} paradas";
    public string EmptyMessage => FilteredPoints.Count == 0 ? "Sin puntos con estos filtros. Activa una capa o cambia la búsqueda." : "";
    public bool NoPoints => FilteredPoints.Count == 0;
    public bool HasSelection => selectedPoint != null;
    public bool HasGuide => selectedGuide != null;
    public bool HasRoute => Plan.State.Route.Count > 0;
    public bool HasSelectedStop => selectedStop != null;
    public bool CanUndo => Plan.CanUndo;
    public string SelectedDetail => selectedPoint == null ? "Pulsa un punto del mapa o elige un resultado." :
        $"{selectedPoint.Name}\n{selectedPoint.Place}\nEstado: {ReviewName(Plan.State.Reviews.GetValueOrDefault(selectedPoint.Id))}\nReferencia {selectedPoint.Id}";
    public string GroupLabel => NearbyPoints.Count > 1 ? $"{NearbyPoints.Count} puntos próximos; elige uno:" : "Punto seleccionado";

    public MapPlannerModel(bool persist = true, string? customPath = null)
    {
        statePath = persist ? customPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ContadorModern",
            $"mapa-{MapCatalog.DatasetId}.json") : null;
        MapPlanState? state = null;
        if (statePath != null && File.Exists(statePath))
        {
            try
            {
                if (new FileInfo(statePath).Length > 1_000_000) throw new InvalidDataException("Archivo demasiado grande.");
                state = JsonSerializer.Deserialize<MapPlanState>(File.ReadAllText(statePath));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                canSave = false;
                SaveMessage = "No se pudo cargar el plan local. No se sobrescribirá ese archivo; revisa la carpeta de datos en Ajustes. El historial del contador está separado.";
            }
        }
        Plan = new MapPlan(state);
        PlaceNames = Catalog.Places.Keys.OrderBy(name => name).ToList();
        Layers = MapCatalog.Categories.Select(category => new MapLayer(category,
            Catalog.Points.Count(p => p.Kind == category.Key), Plan.State.Layers.Contains(category.Key))).ToList();
        foreach (var layer in Layers) layer.PropertyChanged += (_, _) => { Filter(); Save(); };
        Filter();
    }

    public string Search
    {
        get => search;
        set { search = value ?? ""; Filter(); }
    }
    public bool HideReviewed
    {
        get => hideReviewed;
        set { hideReviewed = value; Filter(); }
    }
    public bool EditRoute
    {
        get => editRoute;
        set { editRoute = value; Notify(); }
    }
    public bool HighDetailEnabled
    {
        get => highDetailEnabled;
        set
        {
            if (highDetailEnabled == value) return;
            highDetailEnabled = value;
            Notify();
        }
    }
    public bool ShowRoute
    {
        get => showRoute;
        set { showRoute = value; Notify(); }
    }
    public RouteGuide? SelectedGuide
    {
        get => selectedGuide;
        set
        {
            if (value == null || value == selectedGuide) return;
            selectedGuide = value;
            Plan.SetRoute(Catalog.SuggestedRoute(value));
            ChangedPlan();
        }
    }
    public MapPoint? SelectedPoint
    {
        get => selectedPoint;
        set
        {
            if (selectedPoint == value) return;
            selectedPoint = value;
            Notify();
            Notify(nameof(SelectedDetail));
            Notify(nameof(HasSelection));
        }
    }
    public PlannerRouteRow? SelectedStop
    {
        get => selectedStop;
        set
        {
            if (selectedStop == value) return;
            selectedStop = value;
            Notify();
            Notify(nameof(HasSelectedStop));
        }
    }

    public void SelectGroup(IReadOnlyList<MapPoint> points)
    {
        NearbyPoints = points;
        selectedPoint = points.FirstOrDefault();
        NotifyAll();
    }

    public void AddPointStop()
    {
        if (SelectedPoint is not { } point) return;
        AddStop(point.X, point.Y, point.ListLabel);
    }

    public void AddStop(double x, double y, string? name = null)
    {
        if (!Plan.AddStop(new RouteNode(name ?? $"Mi parada {Plan.State.Route.Count + 1}", x, y)))
        {
            SaveMessage = "La ruta admite hasta 100 paradas dentro del mapa.";
            NotifyAll();
            return;
        }
        selectedGuide = null;
        ChangedPlan();
    }

    public void MoveStop(int index, double x, double y)
    {
        Plan.MoveStop(index, x, y);
        selectedGuide = null;
        ChangedPlan();
    }

    public void RemoveStop()
    {
        if (SelectedStop == null) return;
        Plan.RemoveStop(SelectedStop.Index);
        selectedStop = null;
        selectedGuide = null;
        ChangedPlan();
    }

    public void ClearRoute()
    {
        Plan.SetRoute([]);
        selectedGuide = null;
        selectedStop = null;
        ChangedPlan();
    }

    public void Mark(MapReview review)
    {
        if (SelectedPoint == null) return;
        Plan.Mark(SelectedPoint.Id, review);
        ChangedPlan();
    }

    public void ResetReviews() { Plan.ResetReviews(); ChangedPlan(); }
    public void Undo() { Plan.Undo(); selectedGuide = null; selectedStop = null; ChangedPlan(); }

    private void ChangedPlan()
    {
        Filter();
        Save();
        NotifyAll();
    }

    private void Filter()
    {
        var enabled = Layers.Where(layer => layer.Visible).Select(layer => layer.Key).ToHashSet();
        var query = search.Trim();
        FilteredPoints = Catalog.Points.Where(point => enabled.Contains(point.Kind)
            && (!hideReviewed || !Plan.State.Reviews.ContainsKey(point.Id))
            && (query.Length == 0 || point.ListLabel.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || point.Id.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
        if (selectedPoint != null && !FilteredPoints.Contains(selectedPoint))
        {
            selectedPoint = null;
            NearbyPoints = [];
        }
        NotifyAll();
    }

    private void Save()
    {
        Plan.State.Layers = Layers.Where(layer => layer.Visible).Select(layer => layer.Key).ToList();
        if (statePath == null || !canSave) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
            var tmp = statePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(Plan.State), new UTF8Encoding(false));
            File.Move(tmp, statePath, true);
            SaveMessage = "Plan y marcas guardados solo en este equipo.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            SaveMessage = "No se pudo guardar el plan del mapa. Revisa permisos y espacio en la carpeta de datos de Ajustes.";
        }
        Notify(nameof(SaveMessage));
    }

    private static string ReviewName(MapReview state) => state switch
    {
        MapReview.Checked => "Revisado manualmente",
        MapReview.Empty => "Revisado: no había botín",
        _ => "Pendiente"
    };
    private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    private void NotifyAll() => PropertyChanged?.Invoke(this, new(null));
}
