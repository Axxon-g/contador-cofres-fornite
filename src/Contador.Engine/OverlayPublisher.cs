using ChestCounter;

namespace Contador.Engine;

/// <summary>
/// Publishes immutable HUD snapshots only when their content changes.
/// Publish is called by the UI thread; report generation may read Current from a worker.
/// </summary>
public sealed class OverlayPublisher
{
    private readonly Action<OverlayView> publish;
    private volatile OverlayView current;

    public OverlayPublisher(OverlayView initial, Action<OverlayView> publish)
    {
        current = initial;
        this.publish = publish;
    }

    public OverlayView Current => current;

    public void Publish(OverlayView model, bool force = false)
    {
        if (!force && model.HasSameContent(current))
            return;

        current = model;
        publish(model);
    }

    public void UpdatePresence(bool recording)
    {
        var previous = current;
        if (previous.CurrentLive == recording)
            return;

        Publish(previous with
        {
            CurrentLive = recording,
            Current = recording ? "…" : previous.Last,
            VisualEstimate = null,
            Badge = recording ? "EN PARTIDA" : null
        });
    }
}
