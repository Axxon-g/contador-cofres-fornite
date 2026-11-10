using System.Windows;
namespace Contador;
public partial class App : Application
{
    private Mutex? instance;
    protected override void OnStartup(StartupEventArgs e)
    {
        instance = new Mutex(true, "Local\\Contador.Modern.Desktop", out var created);
        if (!created) { MessageBox.Show("Contador ya está abierto. Búscalo en la barra de tareas.", "Contador"); Shutdown(); return; }
        DispatcherUnhandledException += (_, a) => { MessageBox.Show(a.Exception.Message, "No se pudo completar la operación"); a.Handled = true; };
        base.OnStartup(e);
    }
    protected override void OnExit(ExitEventArgs e) { instance?.Dispose(); base.OnExit(e); }
}
