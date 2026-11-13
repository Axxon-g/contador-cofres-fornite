using System.Windows;
using System.IO;
using System.Windows.Threading;
using Contador;
using ChestCounter;
using Contador.Engine;
using System.Windows.Controls;

namespace Contador.DesktopChecks;
public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        Exception? failure = null;
        application.DispatcherUnhandledException += (_, e) => { failure = e.Exception; e.Handled = true; application.Shutdown(1); };
        application.Startup += async (_, _) =>
        {
            try
            {
                if (args.Contains("--map-ui-only"))
                {
                    await MapUiChecks.Run(application);
                    application.Shutdown();
                    return;
                }
                if (args.Contains("--views-preview"))
                {
                    ViewsPreview.Show(application);
                    return;
                }
                if (args.Length == 2 && args[0] == "--render-views")
                {
                    ViewsPreview.Show(application, args[1]);
                    return;
                }
                await Scenario(false, false);
                await Scenario(true, false);
                await Scenario(false, true);
                await ProfileSelection();
                Console.WriteLine("4 comprobaciones WPF correctas: tres escenarios de cierre y selección de cuentas durante un análisis.");
                application.Shutdown();
            }
            catch (Exception ex) { failure = ex; application.Shutdown(1); }
        };
        application.Run();
        if (failure != null) { Console.Error.WriteLine(failure); return 1; }
        return 0;
    }

    private static async Task ProfileSelection()
    {
        L.Set("es");
        var data = new TrackerData { Matches = [
            new() { EpicId="A", PlayerName="Jugador", SessionId="a", ChestsOpened=5, AnalyzedAtUtc=DateTime.UtcNow },
            new() { EpicId="B", PlayerName="Jugador", SessionId="b", ChestsOpened=12, AnalyzedAtUtc=DateTime.UtcNow }
        ] };
        var config = new Config { Profile="A" };
        var picker = new ComboBox();
        var window = new Window { Content=picker, Width=300, Height=100, Left=-20000, Top=-20000, ShowInTaskbar=false };
        var folder = Path.Combine(Path.GetTempPath(), "ContadorProfiles-"+Guid.NewGuid());
        var store = new Store(folder);
        using var gate = new SemaphoreSlim(0,1); // Replay analysis currently owns the gate.
        var saved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var dashboard = new Dashboard { IsBusy=true };
        ProfilePickerController? controller = null;
        controller = new ProfilePickerController(picker, async key =>
        {
            await gate.WaitAsync();
            try
            {
                config.Profile=key;
                store.SaveConfig(config);
                dashboard.Refresh(data,config,new SessionCounter(DateTime.UtcNow));
                controller!.Refresh(data,config);
                saved.TrySetResult();
            }
            finally { gate.Release(); }
        });
        try
        {
            controller.Refresh(data,config);
            window.Show();
            if(!picker.IsEnabled || picker.Items.Count!=2) throw new Exception("Selector bloqueado durante análisis.");
            picker.SelectedValue="B";
            controller.Refresh(data,config); // Scanner refresh must preserve the pending user choice.
            if((string?)picker.SelectedValue!="B" || config.Profile!="A" || saved.Task.IsCompleted)
                throw new Exception("La elección pendiente se perdió o modificó datos durante el análisis.");
            gate.Release();
            await saved.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await Task.Yield();
            var overlay = new SessionCounter(DateTime.UtcNow).Overlay(Profiles.View(data,config),false);
            if(!picker.IsEnabled || config.Profile!="B" || store.LoadConfig()?.Profile!="B" || dashboard.Total!="12" || Profiles.View(data,config).Matches.Single().EpicId!="B")
                throw new Exception("La cuenta elegida no se guardó o sus estadísticas se mezclaron.");
            picker.SelectedValue="A";
            if(config.Profile!="A" || dashboard.Total!="5") throw new Exception("No permite volver a la primera cuenta.");
            controller.Refresh(new TrackerData(),config);
            if(picker.IsEnabled) throw new Exception("Selector sin cuentas debe quedar inactivo.");
        }
        finally { window.Close(); if(Directory.Exists(folder)) Directory.Delete(folder,true); }
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
