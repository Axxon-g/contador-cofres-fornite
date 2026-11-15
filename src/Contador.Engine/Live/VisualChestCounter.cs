using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace Contador.Engine.Live;

/// <summary>Transient visual estimates. Never writes or adds to replay history.</summary>
public sealed partial class VisualChestCounter
{
    private readonly Queue<int> recent = new();
    private int credited, emptyFrames;
    public int Count { get; private set; }

    [GeneratedRegex(@"^\W*(?:(?:REGISTRO|REGISTRAR|SEARCHED)\s+(?:DE\s+)?)?(?<kind>(?:(?:RARE|SPRITE|SPIRIT|BURIED)\s+)?CHE[S5]TS?|COFRES?(?:\s+(?:RAROS?|ENTERRADOS?|DE(?:L)?\s+ESPIRITU))?)(?:\s+(?:SEARCHED|ABIERTO|REGISTRADO))?\W*(?:[XK×]\s*(?<amount>\d{1,2}))?\W*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ChestLabel();

    [GeneratedRegex(@"\+?\s*\d[\d.,\s]*\s*(?:XP|PE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ExperienceLabel();

    public static int ReadPopup(IEnumerable<string> source)
    {
        var lines = source.Take(40).Select(line => line.Length <= 160 ? Normalize(line) : "").ToArray();
        if (!ExperienceLabel().IsMatch(string.Join(" ", lines))) return 0;
        var kinds = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var line in lines)
        {
            var match = ChestLabel().Match(ExperienceLabel().Replace(line, "").Trim());
            if (!match.Success) continue;
            var amount = match.Groups["amount"].Success ? int.Parse(match.Groups["amount"].Value) : 1;
            if (amount is < 1 or > 20) continue;
            var kind = match.Groups["kind"].Value;
            var key = kind.Contains("RARE") || kind.Contains("RARO") ? "rare"
                : kind.Contains("BURIED") || kind.Contains("ENTERRADO") ? "buried"
                : kind.Contains("SPRITE") || kind.Contains("SPIRIT") || kind.Contains("ESPIRITU") ? "spirit"
                : "normal";
            kinds[key] = Math.Max(kinds.GetValueOrDefault(key), amount);
        }
        return kinds.Values.Sum();
    }

    private static string Normalize(string line) => new string(line.Normalize(NormalizationForm.FormD)
        .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
        .ToArray()).ToUpperInvariant();

    public bool Observe(IEnumerable<string> lines)
    {
        var amount = ReadPopup(lines);
        if (amount == 0)
        {
            // A single OCR miss must not discard evidence from the previous frame.
            if (++emptyFrames >= 4)
            {
                credited = 0;
                recent.Clear();
            }
            return false;
        }
        emptyFrames = 0;
        recent.Enqueue(amount);
        while (recent.Count > 3) recent.Dequeue();
        if (recent.Count < 2) return false;
        // Require two supporting observations, even if the alternate OCR pass loses
        // a multiplier between them. A single inflated reading cannot increase Count.
        var confirmed = recent.OrderByDescending(value => value).Skip(1).First();
        if (confirmed <= credited) return false;
        Count += confirmed - credited;
        credited = confirmed;
        return true;
    }

    public void Reset()
    {
        Count = credited = emptyFrames = 0;
        recent.Clear();
    }
}
