using System.Windows;
using System.IO;
using System.Windows.Threading;
using Contador;

namespace Contador.DesktopChecks;
public static class Program
{
    [STAThread]
    public static int Main()
    {
        var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        Exception? failure = null;
        application.DispatcherUnhandledException += (_, e) => { failure = e.Exception; e.Handled = true; application.Shutdown(1); };
        application.Startup += async (_, _) =>
        {
            try
            {
                await Scenario(false, false);
                await Scenario(true, false);
                await Scenario(false, true);
                Console.WriteLine("3 comprobaciones WPF correctas: cierre inmediato, análisis pendiente y error de guardado.");
                application.Shutdown();
            }
            catch (Exception ex) { failure = ex; application.Shutdown(1); }
        };
        application.Run();
        if (failure != null) { Console.Error.WriteLine(failure); return 1; }
        return 0;
    }

    private static async Task Scenario(bool busy, bool saveFails)
    {
        var window = new Window { Width=100, Height=100, Left=-20000, Top=-20000, ShowInTaskbar=false };
        var gate = new SemaphoreSlim(busy ? 0 : 1,1);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int saves=0, starts=0, errors=0, closings=0;
        bool inClosing=false;
        _ = new WindowCloseController(window, async () =>
        {
            if (inClosing) throw new Exception("Persistencia dentro de Closing.");
            await gate.WaitAsync();
            try { saves++; if(saveFails) throw new IOException("Error simulado"); }
            finally { gate.Release(); }
        }, () => starts++, ex => { if(inClosing) throw new Exception("Diálogo dentro de Closing."); errors++; });
        window.Closing += (_, _) => { inClosing=true; closings++; window.Dispatcher.BeginInvoke(DispatcherPriority.Send,new Action(()=>inClosing=false)); };
        window.Closed += (_, _) => closed.TrySetResult();
        window.Show();
        window.Close(); // A single user request must eventually close without a second click.
        if(busy)
        {
            await Task.Delay(80);
            if(closed.Task.IsCompleted) throw new Exception("Se cerró antes de terminar el análisis.");
            window.Close(); // Repeated clicks while waiting must not save twice.
            gate.Release();
        }
        await closed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        if(starts!=1 || saves!=1 || errors!=(saveFails?1:0) || closings!=(busy?3:2))
            throw new Exception($"Cierre incorrecto: inicios={starts}, guardados={saves}, errores={errors}, Closing={closings}");
        gate.Dispose();
    }
}
