namespace Contador.Engine.Live;

/// <summary>Unverified HUD labels, never used as player identity or count attribution.</summary>
public static class VisualRoster
{
    private static readonly string[] Excluded =
        ["NIVEL", "LEVEL", "ESCUDO", "SHIELD", "SALUD", "HEALTH", "PING", "FPS", "PARTY", "EQUIPO"];

    public static string[] Parse(IEnumerable<string> lines) => lines.Take(40)
        .Select(line => line.Trim())
        .Where(line => line.Length is >= 3 and <= 24
            && line.Any(char.IsLetter)
            && line.All(character => char.IsLetterOrDigit(character) || character is '_' or '-' or '.' or ' ')
            && !Excluded.Any(label => line.Contains(label, StringComparison.OrdinalIgnoreCase)))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Take(4).ToArray();
}
