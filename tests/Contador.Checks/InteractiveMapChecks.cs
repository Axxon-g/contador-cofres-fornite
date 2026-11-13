using System.Text.Json;
using Contador.Engine;

internal static class InteractiveMapChecks
{
    public static void Run(Action<bool, string> check)
    {
        var catalog = MapCatalog.Current;
        check(catalog.SpawnCount == 2145, "Catálogo conserva los 2145 registros publicados");
        check(catalog.Places.Count == 16, "Lugares de referencia separados del botín");
        check(catalog.Points.Select(p => p.Id).Distinct().Count() == catalog.Points.Count, "Identificadores únicos incluso en posiciones repetidas");
        check(catalog.Points.All(p => MapCatalog.ValidPosition(p.X, p.Y)), "Coordenadas dentro del PNG de 2048 píxeles");
        var expected = new Dictionary<string, int>
        {
            ["chest"] = 925, ["rare_chest"] = 40, ["buried_chest"] = 69,
            ["ammo_box"] = 879, ["wanted_poster"] = 141, ["fishing_rod_barrel"] = 47,
            ["loot_cannon"] = 28, ["sprite_shrine"] = 8, ["vault"] = 8
        };
        foreach (var (kind, count) in expected)
            check(catalog.Points.Count(p => p.Kind == kind) == count, $"Cantidad publicada de {kind}");
        check(!MapCatalog.ValidPosition(double.NaN, 3) && !MapCatalog.ValidPosition(3, double.PositiveInfinity), "Rechaza coordenadas no finitas");
        check(!MapCatalog.ValidPosition(-1, 20) && !MapCatalog.ValidPosition(2049, 20), "No fija puntos inválidos al borde");
        foreach (var guide in RouteGuides.All)
        {
            var route = catalog.SuggestedRoute(guide);
            check(route.Count == guide.Stops.Count && route.All(n => MapCatalog.ValidPosition(n.X, n.Y)),
                "Guía usa el marco del nuevo mapa: " + guide.Title);
        }
        var viewport = new MapViewport();
        viewport.Resize(600, 600);
        var first = catalog.Points.First();
        var screen = viewport.ToScreen(first.X, first.Y);
        var roundTrip = viewport.ToMap(screen.X, screen.Y);
        check(Math.Abs(roundTrip.X - first.X) < .0001 && Math.Abs(roundTrip.Y - first.Y) < .0001,
            "Transformación inversa conserva el punto al seleccionar");
        var anchor = viewport.ToMap(400, 350);
        viewport.ZoomAt(2, 400, 350);
        var afterZoom = viewport.ToMap(400, 350);
        check(Math.Abs(anchor.X - afterZoom.X) < .0001 && Math.Abs(anchor.Y - afterZoom.Y) < .0001,
            "Zoom mantiene la coordenada bajo el cursor");
        viewport.ZoomAt(100, 300, 300);
        check(viewport.Zoom == 8, "Límite superior de zoom");
        viewport.Pan(-1_000_000, -1_000_000);
        var edge = viewport.ToMap(600, 600);
        check(edge.X <= 1667.0001 && edge.Y <= 1728.0001, "Arrastre respeta los límites visibles de la isla");
        viewport.ZoomAt(double.NaN, 300, 300);
        check(viewport.Zoom == 8, "Factor inválido no corrompe el zoom");
        viewport.Fit();
        check(viewport.Zoom == 1 && viewport.CenterX == 1078, "Ver isla restaura el encuadre");
        var clusters = MapClustering.Build(catalog.Points.Where(p => p.Kind != "place"), viewport);
        check(clusters.Sum(g => g.Points.Count) == 2145, "Agrupación conserva todos los registros visibles");
        check(clusters.SelectMany((a, i) => clusters.Skip(i + 1).Select(b =>
            Math.Pow(a.ScreenX - b.ScreenX, 2) + Math.Pow(a.ScreenY - b.ScreenY, 2)
                >= Math.Pow((a.Points.Count > 1 ? 14 : 10) + (b.Points.Count > 1 ? 14 : 10) + 3, 2)))
            .All(separated => separated), "Grupos vecinos no superponen sus marcadores");
        var repeated = Enumerable.Range(0, 3).Select(i => first with { Id = "repeat-" + i }).ToArray();
        viewport.Focus(first.X, first.Y, 8);
        check(MapClustering.Build(repeated, viewport).Sum(g => g.Points.Count) == 3,
            "Zoom máximo no elimina registros coincidentes");
        var plan = new MapPlan();
        check(plan.AddStop(new("Inicio", first.X, first.Y)), "Añade una parada");
        check(!plan.AddStop(new("Inválida", -20, 0)) && plan.State.Route.Count == 1, "No persiste paradas inválidas");
        plan.MoveStop(0, first.X + 1, first.Y + 1);
        check(plan.State.Route[0].X == first.X + 1, "Mueve una parada existente");
        plan.Undo();
        check(plan.State.Route[0].X == first.X, "Deshace movimiento");
        plan.Mark(first.Id, MapReview.Checked);
        check(plan.State.Reviews[first.Id] == MapReview.Checked, "Marca manual independiente");
        plan.ResetReviews();
        plan.Undo();
        check(plan.State.Reviews.Count == 1, "Reiniciar revisión se puede deshacer");
        plan.RemoveStop(0);
        plan.Undo();
        check(plan.State.Route.Count == 1, "Quitar parada se puede deshacer");
        plan.SetRoute([]);
        plan.Undo();
        check(plan.State.Route.Count == 1, "Vaciar ruta se puede deshacer");
        var reloaded = new MapPlan(JsonSerializer.Deserialize<MapPlanState>(JsonSerializer.Serialize(plan.State)));
        check(reloaded.State.Route.Count == 1 && reloaded.State.Reviews.Count == 1, "Plan y revisión conservan formato JSON");
        var clean = new MapPlan(new MapPlanState
        {
            Layers = ["inventada"], Reviews = new() { ["missing-id"] = MapReview.Checked },
            Route = [new("mal", double.NaN, 20)]
        });
        check(clean.State.Route.Count == 0 && clean.State.Reviews.Count == 0 && clean.State.Layers.Count == 0,
            "Carga limpia datos desconocidos sin inventar puntos");
        var otherVersion = new MapPlan(new MapPlanState { Dataset = "otra-isla", Route = [new("vieja", 1000, 1000)] });
        check(otherVersion.State.Route.Count == 0, "No mezcla posiciones de otro catálogo");
        var bounded = new MapPlan();
        for (var i = 0; i < 35; i++) bounded.AddStop(new("Parada", first.X, first.Y));
        for (var i = 0; i < 30; i++) bounded.Undo();
        check(bounded.State.Route.Count == 5, "Conserva las últimas treinta acciones al alcanzar el límite de deshacer");
    }
}
