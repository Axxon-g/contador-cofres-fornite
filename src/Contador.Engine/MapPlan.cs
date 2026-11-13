namespace Contador.Engine;

public enum MapReview { Pending, Checked, Empty }

public sealed class MapPlanState
{
    public string Dataset { get; set; } = MapCatalog.DatasetId;
    public List<string> Layers { get; set; } = ["chest", "rare_chest", "buried_chest", "place"];
    public Dictionary<string, MapReview> Reviews { get; set; } = new();
    public List<RouteNode> Route { get; set; } = [];
}

public sealed class MapPlan
{
    private readonly Stack<MapPlanState> undo = new();
    public MapPlanState State { get; private set; }
    public bool CanUndo => undo.Count > 0;

    public MapPlan(MapPlanState? initial = null)
    {
        State = initial ?? new MapPlanState();
        if (State.Dataset != MapCatalog.DatasetId) State = new MapPlanState();
        var ids = MapCatalog.Current.Points.Select(p => p.Id).ToHashSet();
        State.Reviews = (State.Reviews ?? new()).Where(kv => ids.Contains(kv.Key)
            && Enum.IsDefined(kv.Value)).ToDictionary(kv => kv.Key, kv => kv.Value);
        State.Route = (State.Route ?? []).Where(n => n != null && n.Name != null
            && n.Name.Length <= 180 && MapCatalog.ValidPosition(n.X, n.Y)).Take(100).ToList();
        State.Layers = (State.Layers ?? []).Where(key => MapCatalog.Categories.Any(c => c.Key == key)).Distinct().ToList();
    }

    private void BeforeChange()
    {
        // One bounded snapshot per explicit edit; no replay/history data is involved.
        if (undo.Count >= 30)
        {
            var recent = undo.Take(29).Reverse().ToArray();
            undo.Clear();
            foreach (var snapshot in recent) undo.Push(snapshot);
        }
        undo.Push(new MapPlanState
        {
            Layers = State.Layers.ToList(), Reviews = new(State.Reviews), Route = State.Route.ToList()
        });
    }

    public bool AddStop(RouteNode node)
    {
        if (State.Route.Count >= 100 || !MapCatalog.ValidPosition(node.X, node.Y)) return false;
        BeforeChange();
        State.Route.Add(node with { Name = string.IsNullOrWhiteSpace(node.Name) ? "Mi parada" : node.Name[..Math.Min(180, node.Name.Length)] });
        return true;
    }

    public void SetRoute(IEnumerable<RouteNode> nodes)
    {
        var valid = nodes.Where(n => MapCatalog.ValidPosition(n.X, n.Y)).Take(100).ToList();
        BeforeChange();
        State.Route = valid;
    }

    public void Mark(string id, MapReview review)
    {
        if (!MapCatalog.Current.Points.Any(p => p.Id == id) || !Enum.IsDefined(review)) return;
        BeforeChange();
        if (review == MapReview.Pending) State.Reviews.Remove(id);
        else State.Reviews[id] = review;
    }

    public void ResetReviews()
    {
        BeforeChange();
        State.Reviews.Clear();
    }

    public void MoveStop(int index, double x, double y)
    {
        if (index < 0 || index >= State.Route.Count || !MapCatalog.ValidPosition(x, y)) return;
        BeforeChange();
        State.Route[index] = State.Route[index] with { X = x, Y = y };
    }

    public void RemoveStop(int index)
    {
        if (index < 0 || index >= State.Route.Count) return;
        BeforeChange();
        State.Route.RemoveAt(index);
    }

    public void Undo()
    {
        if (!undo.TryPop(out var snapshot)) return;
        // Undo editing, not a subsequent change of visible layers.
        snapshot.Layers = State.Layers;
        State = snapshot;
    }
}
