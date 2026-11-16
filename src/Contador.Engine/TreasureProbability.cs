namespace Contador.Engine;

/// <summary>Independent trials with a constant per-chest rate; never predicts the next drop.</summary>
public static class TreasureProbability
{
    public sealed record Rate(string Name, int OneIn, double TreasureHoursMultiplier);

    // Community reference, not a verified current Epic configuration. See docs/PROBABILIDADES.md.
    public static IReadOnlyList<Rate> ReferenceRates { get; } = Array.AsReadOnly<Rate>(
    [
        new("Ficha pirata", 137_500, 1.5),
        new("Atuendo dorado", 1_562_500, 7.5),
        new("Vehículo y calcomanía", 7_813, 10),
        new("Pico o mochila", 568, 1),
        new("Pista musical", 391, 1),
        new("Pantalla de carga", 272, 1),
        new("Gizmo", 127, 1)
    ]);

    public static double AtLeastOne(long chests, double oneIn)
    {
        if (chests < 0) throw new ArgumentOutOfRangeException(nameof(chests));
        if (!double.IsFinite(oneIn) || oneIn < 1)
            throw new ArgumentOutOfRangeException(nameof(oneIn));
        if (chests == 0) return 0;
        if (oneIn == 1) return 1;

        // Avoid cancellation for rare rewards and small samples.
        var logMiss = double.LogP1(-1.0 / oneIn);
        return Math.Clamp(-double.ExpM1(chests * logMiss), 0, 1);
    }
}
