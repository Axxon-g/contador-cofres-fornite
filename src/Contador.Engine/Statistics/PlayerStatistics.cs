using System.Text.Json;

namespace Contador.Engine.Statistics;

public sealed record PlayerStatisticsQuery(
    string Target,
    bool ByAccountId = false,
    string AccountType = "epic",
    string TimeWindow = "lifetime",
    string Input = "all")
{
    public string NormalizedTarget => ByAccountId && Guid.TryParse(Target, out var id)
        ? id.ToString("N") : Target.Trim();

    public bool IsValid => !Target.Any(char.IsControl) && (ByAccountId
        ? NormalizedTarget.Length == 32 && NormalizedTarget.All(Uri.IsHexDigit)
        : NormalizedTarget.Length is >= 2 and <= 64 && !NormalizedTarget.Any(char.IsControl))
        && AccountType is "epic" or "psn" or "xbl"
        && TimeWindow is "lifetime" or "season"
        && Input is "all" or "keyboardMouse" or "gamepad" or "touch";
}

public sealed record PlayerModeStatistics(
    string Mode, long? Matches, long? Wins, long? Kills,
    double? Kd, double? WinRate, long? MinutesPlayed);

public sealed record PlayerStatisticsSnapshot(
    string AccountName, string AccountId, string TimeWindow, string Input,
    long? BattlePassLevel, IReadOnlyList<PlayerModeStatistics> Modes,
    DateTimeOffset RetrievedAt);

public sealed class PlayerStatisticsException(string message) : Exception(message);

public static class PlayerStatisticsParser
{
    public static PlayerStatisticsSnapshot Parse(ReadOnlyMemory<byte> utf8, PlayerStatisticsQuery query,
        DateTimeOffset retrievedAt)
    {
        try
        {
            using var document = JsonDocument.Parse(utf8, new JsonDocumentOptions { MaxDepth = 12 });
            var root = document.RootElement;
            if (root.GetProperty("status").GetInt32() != 200 || root.GetProperty("version").GetInt32() != 1)
                throw new JsonException();
            if (root.TryGetProperty("retrievedAt", out var stamp) && stamp.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(stamp.GetString(), out var cachedAt)
                && cachedAt <= retrievedAt && retrievedAt - cachedAt < TimeSpan.FromHours(1)) retrievedAt = cachedAt;
            var data = root.GetProperty("data");
            var account = data.GetProperty("account");
            var id = account.GetProperty("id").GetString() ?? "";
            if (id.Length != 32 || !id.All(Uri.IsHexDigit)
                || query.ByAccountId && !string.Equals(id, query.NormalizedTarget, StringComparison.OrdinalIgnoreCase))
                throw new JsonException();
            var name = account.GetProperty("name").GetString() ?? "Jugador";
            if (name.Length > 128 || name.Any(char.IsControl))
                throw new JsonException();
            var modes = new List<PlayerModeStatistics>();
            if (data.TryGetProperty("stats", out var stats) && stats.ValueKind == JsonValueKind.Object
                && stats.TryGetProperty(query.Input, out var input) && input.ValueKind == JsonValueKind.Object)
            {
                foreach (var key in new[] { "overall", "solo", "duo", "squad" })
                {
                    if (!input.TryGetProperty(key, out var mode) || mode.ValueKind != JsonValueKind.Object)
                        continue;
                    var label = key switch
                    {
                        "overall" => "General",
                        "solo" => "Solitario",
                        "duo" => "Dúos",
                        _ => "Escuadrones"
                    };
                    modes.Add(new PlayerModeStatistics(label, Integer(mode, "matches"), Integer(mode, "wins"),
                        Integer(mode, "kills"), Number(mode, "kd"), Number(mode, "winRate", 100), Integer(mode, "minutesPlayed")));
                }
            }
            var level = data.TryGetProperty("battlePass", out var pass) && pass.ValueKind == JsonValueKind.Object
                ? Integer(pass, "level") : null;
            return new PlayerStatisticsSnapshot(name, id, query.TimeWindow, query.Input, level, modes, retrievedAt);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            throw new PlayerStatisticsException("La API devolvió datos no compatibles. Inténtalo más tarde.");
        }
    }

    private static long? Integer(JsonElement element, string property) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number)
            && number is >= 0 and <= 1_000_000_000 ? number : null;

    private static double? Number(JsonElement element, string property, double maximum = 1_000_000_000) =>
        element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number
            && value.TryGetDouble(out var number) && double.IsFinite(number) && number >= 0 && number <= maximum
                ? number : null;
}
