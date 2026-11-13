using System.Runtime.InteropServices;
using System.Windows;

namespace Contador;

public partial class App : Application
{
    private DesktopInstance? instance;

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            instance = new DesktopInstance();
            if (!instance.TryAcquire())
            {
                if (instance.RequestActivation() || RestoreLegacyWindow())
                {
                    Shutdown();
                    return;
                }
                // Allow an older instance that is shutting down to release its lock.
                if (!instance.TryAcquire(1200))
                {
                    MessageBox.Show("Otra instancia de Contador sigue ejecutándose, aunque no tenga una ventana visible. "
                        + "Si acabas de cerrarla, espera a que termine de guardar. Si el aviso persiste, "
                        + "busca Contador en el Administrador de tareas y cierra ese proceso antes de volver a abrirlo.", "Contador");
                    Shutdown();
                    return;
                }
            }
            instance.Listen(() =>
            {
                if (!Dispatcher.HasShutdownStarted)
                    Dispatcher.BeginInvoke(new Action(RestoreMainWindow));
            });
            DispatcherUnhandledException += (_, error) =>
            {
                error.Handled = true;
                MessageBox.Show(error.Exception.Message, "No se pudo completar la operación");
                if (MainWindow == null) Shutdown(1);
            };
            base.OnStartup(e);
            ChestCounter.NativeOverlay.DrawCounterIcon = Contador.Engine.TokenIcon.Draw;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow = new MainWindow();
            MainWindow.Show();
        }
        catch (Exception error)
        {
            MessageBox.Show("No se pudo iniciar Contador.\n\n" + error.Message, "Contador");
            Shutdown(1);
        }
    }

    private void RestoreMainWindow()
    {
        if (MainWindow is not { } window || Dispatcher.HasShutdownStarted) return;
        if (window is MainWindow { IsClosing: true }) return;
        if (!window.IsVisible) window.Show();
        if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
        window.Activate();
    }

    private static bool RestoreLegacyWindow()
    {
        var handle = FindWindow(null, "Contador · Fortnite");
        if (handle == IntPtr.Zero) return false;
        ShowWindowAsync(handle, IsIconic(handle) ? 9 : 5);
        SetForegroundWindow(handle);
        return true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        instance?.Dispose();
        base.OnExit(e);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? className, string windowName);
    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr window, int command);
    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr window);
}
