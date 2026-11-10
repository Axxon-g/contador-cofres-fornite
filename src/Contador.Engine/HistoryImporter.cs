using System.Text.Json;
using ChestCounter;

namespace Contador.Engine;

public sealed record HistoryImportResult(int Added, int Updated, IReadOnlyList<string> Warnings)
{
    public string Summary => Added + Updated == 0 ? "" : $"Historial recuperado: {Added} partidas incorporadas y {Updated} actualizadas.";
}

/// <summary>Imports local legacy histories. Source applications and their settings are never modified.</summary>
public static class HistoryImporter
{
    public static HistoryImportResult Import(Store destination, TrackerData current, string applicationData)
    {
        var warnings = new List<string>();
        var merged = new Dictionary<string, MatchRecord>(StringComparer.Ordinal);
        Validate(current);
        foreach (var match in current.Matches) merged.Add(match.SessionId, match);
        var sources = new List<(string Name, string Text)>();
        foreach (var name in new[] { "ChestCounter", "ChestTracker" })
        {
            var path = Path.Combine(applicationData, name, "data.json");
            if (!File.Exists(path)) continue;
            try
            {
                // Snapshot through a shared read: the legacy app may still be open.
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();
                var source = JsonSerializer.Deserialize<TrackerData>(text) ?? throw new InvalidDataException("Historial vacío.");
                Validate(source);
                // Check conflicts before applying any records from this source.
                foreach (var match in source.Matches)
                    if (merged.TryGetValue(match.SessionId, out var existing)
                        && !string.IsNullOrEmpty(existing.EpicId) && !string.IsNullOrEmpty(match.EpicId)
                        && existing.EpicId != match.EpicId)
                        throw new InvalidDataException("Una partida pertenece a cuentas distintas.");
                var changed = false;
                foreach (var match in source.Matches.Where(m => m.IsBattleRoyale))
                {
                    if (!merged.TryGetValue(match.SessionId, out var existing) || match.AnalyzerVersion > existing.AnalyzerVersion)
                    {
                        merged[match.SessionId] = match;
                        changed = true;
                    }
                }
                if (changed) sources.Add((name, text));
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException or UnauthorizedAccessException or JsonException or NotSupportedException)
            {
                warnings.Add($"No se pudo importar {name}. Se volverá a intentar al abrir la app.");
                ErrorLog.Write($"Import history: {name}", ex);
            }
        }
        var added = merged.Values.Count(m => !current.Matches.Any(old => old.SessionId == m.SessionId));
        var updated = current.Matches.Count(m => !ReferenceEquals(m, merged[m.SessionId]));
        if (added + updated == 0) return new(0, 0, warnings);

        // Backup must succeed before persistence. Backups stay with the destination's data.
        var backup = Path.Combine(destination.Folder, "Backups", "import-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(backup);
        if (File.Exists(destination.DataPath)) File.Copy(destination.DataPath, Path.Combine(backup, "ContadorModern-data.json"));
        if (File.Exists(destination.ConfigPath)) File.Copy(destination.ConfigPath, Path.Combine(backup, "ContadorModern-config.json"));
        foreach (var source in sources) File.WriteAllText(Path.Combine(backup, source.Name + "-data.json"), source.Text);
        var candidate = new TrackerData
        {
            Version = current.Version,
            AnalyzerVersion = current.AnalyzerVersion, // Keep the current reader's upgrade/re-analysis policy.
            CreatedUtc = current.CreatedUtc,
            Matches = merged.Values.ToList(),
            ProcessedFiles = new Dictionary<string, string>(current.ProcessedFiles)
        };
        // Do not import legacy fingerprints: available replays can still be re-read by the newer parser.
        destination.SaveData(candidate);
        current.Matches = candidate.Matches;
        return new(added, updated, warnings);
    }

    private static void Validate(TrackerData data)
    {
        if (data.Version != 1 || data.Matches == null || data.ProcessedFiles == null)
            throw new InvalidDataException("Formato de historial incompatible.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var m in data.Matches)
            if (m == null || string.IsNullOrWhiteSpace(m.SessionId) || !ids.Add(m.SessionId)
                || m.ChestsOpened < 0 || m.Chests == null || m.TreasureFinds == null || m.TeamTreasureFinds == null)
                throw new InvalidDataException("El historial contiene partidas incompletas o duplicadas.");
    }
}
