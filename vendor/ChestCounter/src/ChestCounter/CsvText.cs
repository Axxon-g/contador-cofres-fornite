using System.Globalization;

namespace ChestCounter;

/// <summary>Encodes data as a single spreadsheet-safe cell for either export delimiter.</summary>
public static class CsvText
{
    public static string Cell(object? value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
        var first = 0;
        while (first < text.Length && (char.IsWhiteSpace(text[first]) || char.IsControl(text[first]))) first++;
        if (first < text.Length && "=+-@".Contains(text[first])) text = "'" + text;
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}
