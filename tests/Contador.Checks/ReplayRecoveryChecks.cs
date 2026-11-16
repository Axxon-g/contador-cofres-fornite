using System.Text.Json;
using ChestCounter;
using Contador.Engine;

internal static class ReplayRecoveryChecks
{
    public static void Run(Action<bool, string> check)
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var cancelled = false;
        try
        {
            using var stream = new MemoryStream();
            new ReplayAnalyzer { Cancellation = cancellation.Token }.Analyze(stream, "test.replay", "");
        }
        catch (OperationCanceledException) { cancelled = true; }
        check(cancelled, "Un lector cancelado se detiene antes de decodificar el archivo.");
        var now = DateTime.UtcNow;
        var policy = new ReplayRetryPolicy();
        check(policy.IsDue("file", now), "La primera lectura está disponible.");
        check(policy.Schedule("file", new IOException(), now), "Un fallo de acceso temporal se reintenta.");
        check(!policy.IsDue("file", now.AddSeconds(119)), "El sondeo no repite la lectura cada segundo.");
        check(policy.IsDue("file", now.AddMinutes(2)), "El reintento se habilita en su fecha.");
        check(policy.Schedule("file", new TimeoutException(), now.AddMinutes(2)), "Se permite un segundo reintento.");
        check(!policy.Schedule("file", new TimeoutException(), now.AddMinutes(6)), "La tercera lectura fallida agota los intentos.");
        check(!policy.HasPending, "Un fallo definitivo no queda pendiente para siempre.");
        check(!policy.Schedule("format", new InvalidDataException(), now), "Un formato inválido no entra en un bucle de reintentos.");
        policy.Schedule("success", new IOException(), now);
        policy.Complete("success");
        check(!policy.HasPending, "Un éxito limpia el reintento.");
        policy.Schedule("deleted", new IOException(), now);
        policy.Retain(new HashSet<string>());
        check(!policy.HasPending, "Un archivo eliminado no queda pendiente para siempre.");
        var estimate = MissingMatchEstimate.Calculate(12, 10, 200);
        check(estimate.MissingMatches == 2 && estimate.EstimatedChests == 40, "La estimación usa solo las partidas faltantes y la media propia.");
        check(MissingMatchEstimate.Calculate(5, 10, 200).MissingMatches == 0, "No hay partidas faltantes negativas.");
        var noSample = false;
        try { MissingMatchEstimate.Calculate(10, 0, 0); }
        catch (ArgumentOutOfRangeException) { noSample = true; }
        check(noSample, "No se inventa una media sin partidas registradas.");

        var folder = Path.Combine(Path.GetTempPath(), "ReplayRecovery-" + Guid.NewGuid());
        try
        {
            var demos = Path.Combine(folder, "demos");
            Directory.CreateDirectory(demos);
            var file = new FileInfo(Path.Combine(demos, "nombre-privado.replay"));
            File.WriteAllBytes(file.FullName, [1, 2, 3, 4]);
            file.Refresh();
            var key = Tracker.FileKey(file);
            var archive = Path.Combine(folder, "archive");
            var first = ReplayArchive.Save(file, archive, key);
            var second = ReplayArchive.Save(file, archive, key);
            check(first == second && Directory.GetFiles(archive).Length == 1, "La copia local deduplica por contenido.");
            check(File.ReadAllBytes(first).SequenceEqual(File.ReadAllBytes(file.FullName)), "El archivo conserva el contenido exacto.");
            check(File.Exists(file.FullName), "Archivar no elimina el original.");
            var data = new TrackerData();
            data.ProcessedFiles[key] = "error: No se pudo identificar al jugador de esta repetición";
            data.ProcessedFiles["missing|1|2"] = "error: ruta-privada";
            data.ProcessedFiles["success|1|2"] = "session-private";
            data.Matches.Add(new MatchRecord { SessionId = "session-private", EpicId = "private-epic", PlayerName = "private-player", ChestsOpened = 7 });
            var config = new Config { DemosFolder = demos };
            var json = ReplayDiagnostics.Export(data, config, "2.2.0");
            check(!json.Contains("nombre-privado") && !json.Contains(folder) && !json.Contains("private-player")
                && !json.Contains("private-epic") && !json.Contains("session-private"), "El diagnóstico omite identificadores y rutas.");
            using var document = JsonDocument.Parse(json);
            check(document.RootElement.GetProperty("files")[0].GetProperty("category").GetString() == "player_identity",
                "El diagnóstico distingue errores de identificación.");
            var store = new Store(Path.Combine(folder, "data"));
            var tracker = new Tracker(store, config, data);
            check(tracker.RetryFailedFiles() == 1, "Solo se reintentan errores de archivos que todavía existen.");
            check(data.Matches.Single().ChestsOpened == 7 && data.ProcessedFiles.ContainsKey("success|1|2")
                && data.ProcessedFiles.ContainsKey("missing|1|2"), "El reintento conserva los éxitos y el historial ausente.");
            check(tracker.RetryFailedFiles() == 0, "Preparar dos veces un reintento no duplica el trabajo.");
            File.AppendAllText(file.FullName, "changed");
            var rejected = false;
            try { ReplayArchive.Save(file, archive, key); }
            catch (IOException) { rejected = true; }
            check(rejected && Directory.GetFiles(archive).Length == 1, "Un archivo modificado no se archiva como la copia anterior.");
            check(!new Config().ArchiveReplays, "El archivo local requiere activación explícita.");
        }
        finally { Directory.Delete(folder, true); }
    }
}
