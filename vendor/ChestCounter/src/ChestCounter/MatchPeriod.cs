namespace ChestCounter;

/// <summary>Inclusive local-calendar dates; filters views without removing stored matches.</summary>
public static class MatchPeriod
{
    public static bool IsValid(DateTime? from, DateTime? through) =>
        from == null || through == null || from.Value.Date <= through.Value.Date;

    public static DateTime LocalDate(MatchRecord match, TimeZoneInfo? zone = null)
    {
        var stamp = match.MatchStartUtc ?? match.AnalyzedAtUtc;
        var utc = stamp.Kind == DateTimeKind.Local ? stamp.ToUniversalTime() : DateTime.SpecifyKind(stamp, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, zone ?? TimeZoneInfo.Local).Date;
    }

    public static bool Includes(MatchRecord match, Config config, TimeZoneInfo? zone = null)
    {
        if (!IsValid(config.MatchDateFrom, config.MatchDateThrough)) return false;
        var day = LocalDate(match, zone);
        return (config.MatchDateFrom == null || day >= config.MatchDateFrom.Value.Date)
            && (config.MatchDateThrough == null || day <= config.MatchDateThrough.Value.Date);
    }

    public static string Describe(Config config) => (config.MatchDateFrom, config.MatchDateThrough) switch
    {
        (null, null) => "Todo el historial",
        ({ } from, null) => $"Desde el {from:dd/MM/yyyy}",
        (null, { } through) => $"Hasta el {through:dd/MM/yyyy}",
        ({ } from, { } through) => $"Del {from:dd/MM/yyyy} al {through:dd/MM/yyyy}"
    };
}
