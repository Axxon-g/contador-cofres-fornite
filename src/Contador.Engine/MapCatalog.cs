using System.Text.Json;

namespace Contador.Engine;

public sealed record MapPosition(double X, double Y);
public sealed record MapCategory(string Key, string Name, string Color);
public sealed record MapPoint(string Id, string Kind, string Name, string Place, double X, double Y)
{
    public string ListLabel => string.IsNullOrEmpty(Place) ? Name : $"{Name} · {Place}";
}
public sealed record RouteNode(string Name, double X, double Y);

/// <summary>Fixed public source snapshot. These are possible spawns, never live game data.</summary>
public sealed class MapCatalog
{
    public const string DatasetId = "crossbones-43.10-20261113";
    public const string SourceUrl = "https://lifecrisis.life/loot";
    public const string Version = "43.10";
    public const string Status = "v43.10 · referencia comunitaria · bóvedas corregidas con imagen, orientativas";
    public static IReadOnlyList<MapCategory> Categories { get; } = [
        new("chest", "Cofres", "#FFC857"),
        new("rare_chest", "Cofres raros", "#D7B6FF"),
        new("buried_chest", "Cofres enterrados", "#FFA56B"),
        new("ammo_box", "Cajas de munición", "#A9D5A5"),
        new("wanted_poster", "Carteles", "#F29DA5"),
        new("fishing_rod_barrel", "Barriles de pesca", "#8EBBFF"),
        new("loot_cannon", "Cañones de botín", "#8DDFEC"),
        new("sprite_shrine", "Santuarios", "#EDA8E4"),
        new("vault", "Bóvedas (orientativas)", "#FFF4DF"),
        new("place", "Lugares", "#BDCEE3")
    ];
    private static readonly Lazy<MapCatalog> Instance = new(Load);
    public static MapCatalog Current => Instance.Value;
    public IReadOnlyList<MapPoint> Points { get; private init; } = [];
    public IReadOnlyDictionary<string, MapPosition> Places { get; private init; } = new Dictionary<string, MapPosition>();
    public int SpawnCount => Points.Count(p => p.Kind != "place");

    private static MapCatalog Load()
    {
        using var stream = typeof(MapCatalog).Assembly.GetManifestResourceStream("Contador.Map.Spawns4310")
            ?? throw new InvalidDataException("Falta el catálogo integrado del mapa.");
        using var json = JsonDocument.Parse(stream);
        var root = json.RootElement;
        if (root.GetProperty("version").GetString() != Version
            || root.GetProperty("width").GetInt32() != 2048 || root.GetProperty("height").GetInt32() != 2048)
            throw new InvalidDataException("El catálogo y la imagen deben usar la misma versión y escala.");
        var types = root.GetProperty("types").EnumerateArray().Select(t => t.GetString()!).ToArray();
        var names = root.GetProperty("places").EnumerateArray().Select(p => p.GetString()!).ToArray();
        var places = root.GetProperty("locations").EnumerateObject().ToDictionary(
            p => p.Name, p => new MapPosition(p.Value[0].GetDouble(), p.Value[1].GetDouble()));
        var points = new List<MapPoint>();
        var index = 0;
        foreach (var row in root.GetProperty("points").EnumerateArray())
        {
            var kindIndex = row[0].GetInt32();
            var x = row[1].GetDouble();
            var y = row[2].GetDouble();
            var placeIndex = row[3].GetInt32();
            if (kindIndex < 0 || kindIndex >= types.Length || !ValidPosition(x, y)
                || placeIndex < -1 || placeIndex >= names.Length)
                throw new InvalidDataException("Coordenada o categoría fuera de los límites del catálogo.");
            var kind = types[kindIndex];
            var category = Categories.Single(c => c.Key == kind);
            var id = $"{DatasetId}:{index++}";
            // Keep IDs of other layers stable; the source's eight vault positions were contradicted.
            if (kind == "vault") continue;
            points.Add(new MapPoint(id, kind, category.Name,
                placeIndex >= 0 ? names[placeIndex] : "Fuera de lugares con nombre", x, y));
        }
        using var vaultStream = typeof(MapCatalog).Assembly.GetManifestResourceStream("Contador.Map.VaultReferences4310")
            ?? throw new InvalidDataException("Falta la referencia corregida de bóvedas.");
        using var vaultJson = JsonDocument.Parse(vaultStream);
        var reference = vaultJson.RootElement;
        foreach (var row in reference.GetProperty("points").EnumerateArray())
        {
            var x = row.GetProperty("x").GetDouble() * reference.GetProperty("scaleX").GetDouble()
                + reference.GetProperty("offsetX").GetDouble();
            var y = row.GetProperty("y").GetDouble() * reference.GetProperty("scaleY").GetDouble()
                + reference.GetProperty("offsetY").GetDouble();
            if (!ValidPosition(x, y)) throw new InvalidDataException("Referencia de bóveda fuera del mapa.");
            points.Add(new MapPoint($"vault-reference-20261113:{row.GetProperty("id").GetString()}",
                "vault", "Bóveda · referencia visual aproximada", row.GetProperty("name").GetString()!, x, y));
        }
        foreach (var (name, position) in places)
        {
            if (!ValidPosition(position.X, position.Y)) throw new InvalidDataException("Lugar inválido.");
            points.Add(new MapPoint($"place:{name}", "place", name, "Lugar de referencia", position.X, position.Y));
        }
        return new MapCatalog { Points = points.AsReadOnly(), Places = places };
    }

    public static bool ValidPosition(double x, double y) =>
        double.IsFinite(x) && double.IsFinite(y) && x is >= 0 and <= 2048 && y is >= 0 and <= 2048;

    public IReadOnlyList<RouteNode> SuggestedRoute(RouteGuide guide) => guide.Stops.Select(stop =>
    {
        if (Places.TryGetValue(stop.Name, out var position)) return new RouteNode(stop.Name, position.X, position.Y);
        if (stop.Name == "Islote al sur" && Places.TryGetValue("The Chest", out position))
            return new RouteNode(stop.Name + " (orientativo)", position.X + 45, position.Y + 90);
        if (stop.Name.Contains("Plunder Port") && Places.TryGetValue("Plunder Port", out position))
            return new RouteNode(stop.Name + " (orientativo)", position.X, position.Y - 90);
        throw new InvalidDataException("La parada no tiene una referencia documentada.");
    }).ToList();
}
