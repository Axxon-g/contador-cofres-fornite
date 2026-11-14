using ChestCounter;
using Contador.Engine;

internal static class QuietRefreshChecks
{
    public static void Run(Action<bool, string> check)
    {
        var view = new OverlayView("42", "4", "última partida", false, null,
            [new OverlayRow("Sesión", "4")], "2", "4", "4") { LastTeamChests = "≥ 8" };
        var same = view with { Rows = view.Rows.ToList() };
        var publications = new List<OverlayView>();
        var publisher = new OverlayPublisher(view, publications.Add);
        publisher.Publish(same);
        check(publications.Count == 0, "El coordinador omite publicaciones idénticas");
        publisher.UpdatePresence(true);
        publisher.UpdatePresence(true);
        check(publications.Count == 1 && publisher.Current.Total == view.Total
            && publisher.Current.CurrentLive, "La presencia cambia una vez y conserva el recuento");
        publisher.UpdatePresence(false);
        check(publications.Count == 2 && publisher.Current.Current == view.Last,
            "Al terminar la presencia recupera la última partida sin leer datos mutables");
        publisher.Publish(publisher.Current, force: true);
        check(publications.Count == 3, "La activación del overlay permite una publicación inicial forzada");
        check(view.HasSameContent(same), "Listas nuevas con idéntico contenido no repintan el overlay");
        check(!view.HasSameContent(view with { Total = "43" }), "Un total nuevo sí se publica");
        check(!view.HasSameContent(view with { CurrentLive = true }), "Cambio de presencia sí se publica");
        check(!view.HasSameContent(view with { LastTeamChests = "≥ 9" }), "Cambio de equipo sí se publica");
        check(!view.HasSameContent(view with { Rows = [new OverlayRow("Sesión", "5")] }), "Cambio de fila clásica sí se publica");

        var folder = Path.Combine(Path.GetTempPath(), "QuietRefresh-" + Guid.NewGuid());
        try
        {
            var store = new Store(folder);
            ObsHud.Write(store, view);
            var dataPath = Path.Combine(folder, "overlay-data.js");
            var sentinel = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            File.SetLastWriteTimeUtc(dataPath, sentinel);
            File.SetLastWriteTimeUtc(store.OverlayPath, sentinel);
            var previousData = File.ReadAllText(dataPath);
            ObsHud.Write(store, same);
            check(File.GetLastWriteTimeUtc(dataPath) == sentinel, "Sondeo sin cambios no reescribe datos OBS");
            check(File.GetLastWriteTimeUtc(store.OverlayPath) == sentinel, "Sondeo sin cambios no reescribe HTML OBS");
            ObsHud.Write(store, view with { Total = "43", CurrentLive = true });
            check(File.ReadAllText(dataPath) != previousData && File.ReadAllText(dataPath).Contains("\"inMatch\":true"),
                "Datos nuevos actualizan números y presencia");
            check(File.GetLastWriteTimeUtc(store.OverlayPath) == sentinel,
                "Los números nuevos no fuerzan recarga del documento OBS");
            Directory.CreateDirectory(Path.Combine(folder, "demos"));
            var tracker = new Tracker(store, new Config { DemosFolder = Path.Combine(folder, "demos") }, new TrackerData());
            var starts = 0;
            tracker.AnalysisStarting = () => starts++;
            tracker.Scan();
            tracker.Scan();
            check(starts == 0, "Revisiones sin archivos no anuncian un análisis");
        }
        finally { if (Directory.Exists(folder)) Directory.Delete(folder, true); }
    }
}
