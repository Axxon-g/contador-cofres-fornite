namespace Contador;

/// <summary>The existence of a named mutex is not evidence that another thread owns it.</summary>
public sealed class DesktopInstance : IDisposable
{
    private readonly Mutex mutex;
    private readonly string activationName;
    private EventWaitHandle? activation;
    private RegisteredWaitHandle? listener;
    private bool ownsMutex;
    private bool disposed;

    public DesktopInstance(string name = "Local\\Contador.Modern.Desktop")
    {
        mutex = new Mutex(false, name);
        activationName = name + ".Activate";
    }

    public bool TryAcquire(int milliseconds = 0)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (ownsMutex) return true;
        try { ownsMutex = mutex.WaitOne(milliseconds); }
        catch (AbandonedMutexException) { ownsMutex = true; }
        return ownsMutex;
    }

    public void Listen(Action showWindow)
    {
        if (!ownsMutex) throw new InvalidOperationException("Solo la instancia principal puede escuchar.");
        if (listener != null) return;
        activation = new EventWaitHandle(false, EventResetMode.AutoReset, activationName);
        listener = ThreadPool.RegisterWaitForSingleObject(activation,
            (_, _) => showWindow(), null, Timeout.Infinite, executeOnlyOnce: false);
    }

    public bool RequestActivation()
    {
        try
        {
            using var existing = EventWaitHandle.OpenExisting(activationName);
            return existing.Set();
        }
        catch (WaitHandleCannotBeOpenedException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        listener?.Unregister(null);
        activation?.Dispose();
        if (ownsMutex)
        {
            mutex.ReleaseMutex();
            ownsMutex = false;
        }
        mutex.Dispose();
    }
}
