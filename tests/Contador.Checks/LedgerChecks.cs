using ChestCounter;

internal static class LedgerChecks
{
    public static void Run(Action<bool, string> check)
    {
        static int Count(ContainerInteractionLedger ledger, uint pawn) => ledger.Events
            .Count(entry => entry.Pawn == pawn && ContainerInteractionLedger.IsCountable(entry));

        var ledger = new ContainerInteractionLedger();
        ledger.OpenChannel(1, "level:chest-A", 0);
        ledger.Observe(1, "Tiered_Chest", 1, false, false, null);
        ledger.Observe(1, "Tiered_Chest", 2, true, false, 10);
        ledger.Observe(1, "Tiered_Chest", 3, true, false, 10);
        check(Count(ledger, 10) == 1, "Una apertura replicada varias veces cuenta una sola vez");
        ledger.Observe(1, "Tiered_Chest", 4, null, true, 20);
        check(ledger.Events.Count == 1, "Destruir un cofre ya abierto no vuelve a contarlo");

        ledger.OpenChannel(2, "level:chest-A", 20);
        ledger.Observe(2, "Tiered_Chest", 20.1f, true, false, 20);
        check(ledger.Events.Count == 1, "Otro canal y otro jugador no prueban una nueva apertura");
        ledger.Observe(2, "Tiered_Chest", 21, false, false, null);
        ledger.Observe(2, "Tiered_Chest", 22, true, false, 10);
        check(Count(ledger, 10) == 2, "Un cierre observado permite contar el mismo cofre rellenado");

        ledger.OpenChannel(3, "level:chest-B", 0);
        ledger.Observe(3, "Tiered_Chest", 10, false, true, 20);
        check(Count(ledger, 20) == 1 && ledger.Events.Last().Destroyed,
            "Cofre cerrado destruido se cuenta aunque bAlreadySearched sea falso");
        ledger.Observe(3, "Tiered_Chest", 11, null, true, 20);
        check(Count(ledger, 20) == 1, "Una destrucción repetida no añade cofres");
        ledger.OpenChannel(30, "level:destroy-late-owner", 0);
        ledger.Observe(30, "Tiered_Chest", 10, false, true, null);
        ledger.Observe(30, "Tiered_Chest", 10.2f, null, false, 70);
        check(Count(ledger, 70) == 1 && ledger.Events.Last().Destroyed,
            "Cofre roto conserva la destrucción al recibir su autor en otro paquete");
        ledger.Observe(30, "Tiered_Chest", 11, true, true, 70);
        check(Count(ledger, 70) == 1, "Un cofre roto y buscado en estados sucesivos solo se cuenta una vez");

        ledger.OpenChannel(4, "level:chest-C", 0);
        ledger.Observe(4, "Tiered_Chest", 10, true, false, null);
        ledger.Observe(4, "Tiered_Chest", 11, null, false, 30);
        check(Count(ledger, 30) == 1 && ledger.Events.Last().LateAttribution,
            "Resuelve SearchingPawn recibido en un paquete posterior del mismo ciclo");

        ledger.OpenChannel(5, "level:chest-D", 0);
        ledger.Observe(5, "Tiered_Chest", 10, true, false, null);
        ledger.Observe(5, "Tiered_Chest", 16, null, false, 30);
        check(ledger.Events.Last().Pawn == null, "Un visitante tardío no hereda una apertura sin autor");
        ledger.OpenChannel(6, "level:chest-D", 20);
        ledger.Observe(6, "Tiered_Chest", 20.1f, true, false, 30);
        check(ledger.Events.Last().Pawn == null, "Reabrir el canal no atribuye un evento pendiente anterior");

        ledger.OpenChannel(7, "guid:77", 0);
        ledger.Observe(7, "Tiered_Chest", 0.1f, true, false, 40);
        check(Count(ledger, 40) == 0, "Un snapshot sin identidad estable no aumenta el total");
        ledger.OpenChannel(8, "level:chest-E", 0);
        ledger.Observe(8, "Tiered_Chest", 0.1f, true, false, 50);
        check(Count(ledger, 50) == 1, "Un snapshot estable usa la misma política para propio y compañero");

        ledger.OpenChannel(9, "level:chest-F", 0);
        ledger.Observe(9, "Tiered_Chest", 1, null, false, 60);
        ledger.Observe(9, "Tiered_Chest", 2, true, false, null);
        ledger.Observe(9, "Tiered_Chest", 3, false, false, null);
        ledger.Observe(9, "Tiered_Chest", 4, true, false, null);
        check(ledger.Events.Last().Pawn == 60,
            "Un delta sin SearchingPawn conserva su valor replicado al reabrir un cofre rellenado");
        check(ledger.Events.Last().Time == 4, "Cada relleno conserva su propio momento");
        ledger.Observe(9, "Tiered_Chest", 5, false, false, 0);
        ledger.Observe(9, "Tiered_Chest", 6, true, false, null);
        check(ledger.Events.Last().Pawn == null, "Un cero explícito sí borra al autor replicado");

        var invalidTimeRejected = false;
        try { ledger.Observe(9, "Tiered_Chest", float.NaN, true, false, 60); }
        catch (InvalidDataException) { invalidTimeRejected = true; }
        check(invalidTimeRejected, "No acepta tiempos NaN en datos de replay");
    }
}
