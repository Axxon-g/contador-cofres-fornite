namespace ChestCounter;

/// <summary>
/// One interaction per observed container cycle, independent of player or team selection.
/// Channel re-entry is a snapshot, not evidence of a refill. Only a closed state rearms it.
/// </summary>
public sealed class ContainerInteractionLedger
{
    public sealed record Interaction(
        float Time, string Key, string Label, uint? Pawn, bool Destroyed,
        bool InitialSnapshot, ChestPosition? Position, bool LateAttribution = false);

    private sealed class ContainerState
    {
        public bool Consumed;
        public int EventIndex = -1;
        public long EventChannel;
    }

    private sealed class ChannelState(string key, float openedAt, long generation)
    {
        public string Key { get; } = key;
        public float OpenedAt { get; } = openedAt;
        public long Generation { get; } = generation;
        public uint? Pawn;
    }

    private readonly Dictionary<string, ContainerState> containers = new(StringComparer.Ordinal);
    private readonly Dictionary<uint, ChannelState> channels = new();
    private readonly List<Interaction> events = new();
    private long generation;

    public IReadOnlyList<Interaction> Events => events;

    public static bool HasStableIdentity(string key) =>
        !key.StartsWith("guid:", StringComparison.Ordinal)
        && !key.StartsWith("ch:", StringComparison.Ordinal);

    public static bool IsCountable(Interaction entry) =>
        !entry.InitialSnapshot || HasStableIdentity(entry.Key);

    public void OpenChannel(uint channel, string key, float time)
    {
        channels[channel] = new ChannelState(key, time, ++generation);
    }

    public void Observe(uint channel, string label, float time, bool? searched,
        bool destroyed, uint? pawn, ChestPosition? position = null)
    {
        if (!float.IsFinite(time) || time < 0)
            throw new InvalidDataException("Tiempo de interacción no válido en la repetición.");
        if (!channels.TryGetValue(channel, out var source))
        {
            OpenChannel(channel, $"ch:{channel}", time);
            source = channels[channel];
        }
        if (!containers.TryGetValue(source.Key, out var state))
        {
            state = new ContainerState();
            containers.Add(source.Key, state);
        }

        // A closed chest being destroyed is an interaction, not a refill notification.
        if (searched == false && !destroyed && state.Consumed)
        {
            state.Consumed = false;
            state.EventIndex = -1;
        }
        if (pawn is uint identity)
        {
            source.Pawn = identity == 0 ? null : identity;
        }

        if (state.Consumed)
        {
            if (state.EventIndex >= 0 && state.EventChannel == source.Generation)
            {
                var previous = events[state.EventIndex];
                // Reconcile a split property update only within this channel and cycle.
                // A later visitor or a reopened channel must not inherit an unknown search.
                if (previous.Pawn == null && pawn is > 0
                    && time >= previous.Time && time - previous.Time <= 5)
                {
                    events[state.EventIndex] = previous with { Pawn = pawn, LateAttribution = true };
                }
            }
            return;
        }
        if (searched != true && !destroyed) return;

        // SearchingPawn is a replicated property, not an event argument. Omission in
        // a delta means unchanged: the same player may reopen a refilled chest minutes
        // later. Only an explicit zero or a new channel clears that property's value.
        var opener = source.Pawn;
        state.Consumed = true;
        state.EventIndex = events.Count;
        state.EventChannel = source.Generation;
        events.Add(new Interaction(time, source.Key, label, opener, destroyed,
            time - source.OpenedAt < 0.5f, position?.IsValid == true ? position : null));
    }
}
