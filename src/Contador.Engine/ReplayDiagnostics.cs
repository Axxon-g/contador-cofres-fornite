using System.Text.Json;
using ChestCounter;

namespace Contador.Engine;

/// <summary>Support export deliberately omitting paths, player names, account IDs and exception text.</summary>
public static class ReplayDiagnostics
{
    public static string Category(string value)
    {
        if (value.Contains("identificar al jugador", StringComparison.OrdinalIgnoreCase)) return "player_identity";
        if (value.Contains("demasiado largo", StringComparison.OrdinalIgnoreCase)) return "timeout";
        if (value.Contains("incomplete", StringComparison.OrdinalIgnoreCase)) return "incomplete";
        if (value.StartsWith("error:", StringComparison.Ordinal)) return "unreadable";
        return "processed";
    }

    public static string Export(TrackerData data, Config config, string appVersion)
    {
        var directory = new DirectoryInfo(config.DemosFolder);
        var files = directory.Exists ? directory.EnumerateFiles("*.replay").ToArray() : [];
        var records = files.Select(file => new
        {
            bytes = file.Length,
            modifiedUtc = file.LastWriteTimeUtc,
            live = ReplayHeaderProbe.IsLive(file.FullName),
            category = data.ProcessedFiles.TryGetValue(Tracker.FileKey(file), out var state)
                ? Category(state) : "pending"
        }).ToArray();
        return JsonSerializer.Serialize(new
        {
            schema = 1,
            appVersion,
            analyzerVersion = ReplayAnalyzer.CurrentVersion,
            generatedUtc = DateTime.UtcNow,
            directoryExists = directory.Exists,
            replayCount = files.Length,
            files = records,
            historicalErrors = data.ProcessedFiles.Values.Count(value => value.StartsWith("error:", StringComparison.Ordinal))
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
