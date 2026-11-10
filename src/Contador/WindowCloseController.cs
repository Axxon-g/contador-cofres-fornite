using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Contador;

/// <summary>Completes persistence after WPF has unwound the original Closing event.</summary>
public sealed class WindowCloseController
{
    private readonly Window window;
    private readonly Func<Task> save;
    private readonly Action begin;
    private readonly Action<Exception> failed;
    private bool requested, allowClose;

    public WindowCloseController(Window window, Func<Task> save, Action begin, Action<Exception> failed)
    {
        this.window = window; this.save = save; this.begin = begin; this.failed = failed;
        window.Closing += OnClosing;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (allowClose) return;
        e.Cancel = true;
        if (requested) return;
        requested = true;
        begin();
        // WaitAsync can complete synchronously. Always defer the entire operation to avoid
        // calling Close (or showing an owned dialog) inside the original Closing event.
        window.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
        {
            try { await save(); }
            catch (Exception ex) { failed(ex); }
            finally { allowClose = true; window.Close(); }
        }));
    }
}
