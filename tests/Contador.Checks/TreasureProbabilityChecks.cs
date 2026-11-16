using Contador.Engine;

internal static class TreasureProbabilityChecks
{
    public static void Run(Action<bool, string> check)
    {
        check(TreasureProbability.AtLeastOne(0, 137500) == 0, "Sin intentos no hay probabilidad acumulada.");
        check(Math.Abs(TreasureProbability.AtLeastOne(1, 137500) - 1.0 / 137500) < 1e-15,
            "Un cofre conserva la tasa individual.");
        check(Math.Abs(TreasureProbability.AtLeastOne(2, 2) - 0.75) < 1e-12,
            "Dos intentos independientes de 50 % dan 75 %.");
        check(TreasureProbability.AtLeastOne(1, 1) == 1, "Un evento seguro ocurre con un intento.");
        check(TreasureProbability.AtLeastOne(10000, 137500) > TreasureProbability.AtLeastOne(500, 137500),
            "La probabilidad acumulada es creciente.");
        check(TreasureProbability.AtLeastOne(500, 137500 / 1.5) > TreasureProbability.AtLeastOne(500, 137500),
            "Una tasa mejorada aumenta la probabilidad.");
        check(double.IsFinite(TreasureProbability.AtLeastOne(long.MaxValue, 137500)),
            "Las cantidades grandes no producen infinito o NaN.");
        check(TreasureProbability.AtLeastOne(1, 1e16) > 0, "Las tasas muy pequeñas conservan precisión.");
        foreach (var invalid in new[] { double.NaN, double.PositiveInfinity, 0, -1, 0.5 })
        {
            var rejected = false;
            try { TreasureProbability.AtLeastOne(1, invalid); }
            catch (ArgumentOutOfRangeException) { rejected = true; }
            check(rejected, "Se rechazan tasas inválidas.");
        }
        var negativeRejected = false;
        try { TreasureProbability.AtLeastOne(-1, 127); }
        catch (ArgumentOutOfRangeException) { negativeRejected = true; }
        check(negativeRejected, "Se rechazan cantidades negativas.");
    }
}
