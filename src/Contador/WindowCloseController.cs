using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Contador;

public enum WindowCloseChoice { Cancel, Background, Exit }

/// <summary>Completes persistence after WPF has unwound the original Closing event.</summary>
public sealed class WindowCloseController
{
    private readonly Window window;
    private readonly Func<Task> save;
    private readonly Action begin;
    private readonly Action<Exception> failed;
    private readonly Func<WindowCloseChoice>? choose;
    private readonly Action? background;
    private bool requested, allowClose, forceExit;

    public WindowCloseController(Window window, Func<Task> save, Action begin, Action<Exception> failed,
        Func<WindowCloseChoice>? choose = null, Action? background = null)
    {
        this.window = window; this.save = save; this.begin = begin; this.failed = failed;
        this.choose = choose;
        this.background = background;
        window.Closing += OnClosing;
    }

    public void RequestExit()
    {
        forceExit = true;
        window.Close();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (allowClose) return;
        e.Cancel = true;
        if (requested) return;
        requested = true;
        // WaitAsync can complete synchronously. Always defer the entire operation to avoid
        // calling Close (or showing an owned dialog) inside the original Closing event.
        window.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
        {
            WindowCloseChoice choice;
            try { choice = forceExit ? WindowCloseChoice.Exit : choose?.Invoke() ?? WindowCloseChoice.Exit; }
            catch (Exception ex) { requested = false; failed(ex); return; }
            if (forceExit) choice = WindowCloseChoice.Exit;
            if (choice == WindowCloseChoice.Cancel) { requested = false; return; }
            if (choice == WindowCloseChoice.Background)
            {
                try { if (background != null) background(); else window.Hide(); }
                catch (Exception ex) { failed(ex); }
                finally { requested = false; }
                return;
            }
            try { begin(); await save(); }
            catch (Exception ex) { failed(ex); }
            finally { allowClose = true; window.Close(); }
        }));
    }
}
