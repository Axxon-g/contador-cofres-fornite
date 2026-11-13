using Contador;

namespace Contador.DesktopChecks;

internal static class InstanceChecks
{
    public static void Run()
    {
        var checks = 0;
        void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            checks++;
        }
        var name = "Local\\Contador.Test." + Guid.NewGuid();
        using (var retainedHandle = new Mutex(false, name))
        {
            using var primary = new DesktopInstance(name);
            Check(primary.TryAcquire(), "Un objeto existente sin propietario no debe impedir el inicio.");
            Check(primary.TryAcquire(), "Repetir la comprobación no debe adquirir el mutex recursivamente.");
        }
        using (var reopened = new DesktopInstance(name))
            Check(reopened.TryAcquire(), "El cierre libera el bloqueo para el siguiente inicio.");

        var occupiedName = name + ".Occupied";
        using var ready = new ManualResetEventSlim();
        using var stop = new ManualResetEventSlim();
        using var activated = new ManualResetEventSlim();
        var owner = new Thread(() =>
        {
            using var primary = new DesktopInstance(occupiedName);
            if (!primary.TryAcquire()) return;
            primary.Listen(() => activated.Set());
            ready.Set();
            stop.Wait(TimeSpan.FromSeconds(8));
        });
        owner.Start();
        try
        {
            Check(ready.Wait(TimeSpan.FromSeconds(3)), "La primera instancia debe iniciar su escucha.");
            using (var secondary = new DesktopInstance(occupiedName))
            {
                Check(!secondary.TryAcquire(), "Una instancia realmente activa bloquea la segunda.");
                Check(secondary.RequestActivation() && activated.Wait(TimeSpan.FromSeconds(3)),
                    "El segundo inicio solicita mostrar la ventana existente.");
            }
            using var third = new DesktopInstance(occupiedName);
            Check(!third.TryAcquire(), "Cerrar la instancia secundaria no libera el bloqueo de la principal.");
        }
        finally
        {
            stop.Set();
            owner.Join();
        }
        Mutex? abandonedHandle = null;
        var abandonedName = name + ".Abandoned";
        var crashedOwner = new Thread(() => abandonedHandle = new Mutex(true, abandonedName));
        crashedOwner.Start();
        crashedOwner.Join();
        try
        {
            using var recovered = new DesktopInstance(abandonedName);
            Check(recovered.TryAcquire(), "Un propietario terminado permite recuperar el bloqueo abandonado.");
        }
        finally { abandonedHandle?.Dispose(); }
        Console.WriteLine($"{checks} comprobaciones de instancia única correctas.");
    }
}
