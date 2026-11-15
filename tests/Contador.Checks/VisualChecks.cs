using Contador.Engine.Live;

internal static class VisualChecks
{
    public static void Run(Action<bool, string> check)
    {
        check(VisualChestCounter.ReadPopup(["+100 XP", "CHEST"]) == 1, "Reconoce aviso inglés con experiencia");
        check(VisualChestCounter.ReadPopup(["+100 PE", "COFRE X2"]) == 2, "Reconoce aviso español apilado");
        check(VisualChestCounter.ReadPopup(["+900 XP", "RARE CHEST"]) == 1, "Reconoce un cofre raro inglés");
        check(VisualChestCounter.ReadPopup(["+900 PE", "COFRE DE ESPÍRITU"]) == 1, "Reconoce variantes españolas con acento");
        check(VisualChestCounter.ReadPopup(["+200 PE COFRE X2"]) == 2, "Lee experiencia y cofre unidos por OCR");
        check(VisualChestCounter.ReadPopup(["+100 PE", "REGISTRO DE COFRE"]) == 1, "Reconoce registro de cofre en español");
        check(VisualChestCounter.ReadPopup(["+200 XP", "CHEST", "RARE CHEST"]) == 2, "Suma distintos tipos en el mismo aviso");
        check(VisualChestCounter.ReadPopup(["+400 XP", "CHEST X2", "SPRITE CHEST X2"]) == 4, "Suma multiplicadores por tipo, no solo el mayor");
        check(VisualChestCounter.ReadPopup(["+200 XP", "RARE CHEST X2", "RARE CHEST X2"]) == 2, "Una línea OCR duplicada no duplica cofres");
        check(VisualChestCounter.ReadPopup(["+100", "PE", "COFRE"]) == 1, "Tolera XP y número separados en líneas");
        check(VisualChestCounter.ReadPopup(["CHEST"]) == 0, "No cuenta una etiqueta sin aviso XP");
        check(VisualChestCounter.ReadPopup(["+100 XP", "PLAYER CHEST"]) == 0, "No cuenta palabras de otro mensaje");
        check(VisualChestCounter.ReadPopup(["+100 XP", "COFRE X99"]) == 0, "Rechaza multiplicadores fuera de límite");
        var counter = new VisualChestCounter();
        check(!counter.Observe(["+100 XP", "CHEST"]) && counter.Count == 0, "Un fotograma aislado no cuenta");
        check(counter.Observe(["+100 XP", "CHEST"]) && counter.Count == 1, "Dos observaciones confirman un aviso visual");
        for (var index = 0; index < 10; index++) counter.Observe(["+100 XP", "CHEST"]);
        check(counter.Count == 1, "El aviso que sigue visible no se duplica");
        counter.Observe([]);
        counter.Observe(["+100 XP", "CHEST"]);
        counter.Observe(["+100 XP", "CHEST"]);
        check(counter.Count == 1, "Un fallo breve de OCR no rearma el mismo aviso");
        counter.Observe(["+200 XP", "CHEST X2"]);
        counter.Observe(["+200 XP", "CHEST X2"]);
        check(counter.Count == 2, "Un aviso apilado añade solo la diferencia");
        for (var index = 0; index < 4; index++) counter.Observe([]);
        counter.Observe(["+100 PE", "COFRE"]);
        counter.Observe(["+100 PE", "COFRE"]);
        check(counter.Count == 3, "Un nuevo aviso tras desaparecer sí se cuenta");
        counter.Reset();
        check(counter.Count == 0, "Cada partida comienza con una estimación independiente");
        counter.Observe(["+100 PE", "COFRE"]);
        counter.Observe([]);
        counter.Observe(["+100 PE", "COFRE"]);
        check(counter.Count == 1, "Dos lecturas con un fallo intermedio confirman el aviso breve");
        counter.Reset();
        counter.Observe(["+200 XP", "CHEST X2"]);
        counter.Observe(["+200 XP", "CHEST"]);
        counter.Observe(["+200 XP", "CHEST X2"]);
        check(counter.Count == 2, "Recupera multiplicadores aunque el filtro intermedio pierda la cifra");
        counter.Observe(["+200 XP", "CHEST X9"]);
        check(counter.Count == 2, "Una cifra OCR inflada aislada no incrementa el contador");
        check(VisualRoster.Parse(["LEVEL 100", "100", "Amigo_1", "Amigo_1", "NIVEL 30"]).SequenceEqual(["Amigo_1"]),
            "Filtra etiquetas y duplicados del HUD sin usarlos como identidad");
        check(VisualRoster.Parse(["Persona1", "Persona2", "Persona3", "Persona4", "Persona5"]).Length == 4,
            "Limita los nombres visuales del equipo");
    }
}
