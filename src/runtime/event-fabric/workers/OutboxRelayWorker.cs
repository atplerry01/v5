namespace Whyce.Runtime.EventFabric.Workers;

/// <summary>
/// Outbox Relay Worker — polls the outbox and relays events to
/// external infrastructure adapters (e.g., Kafka).
/// This is the ONLY bridge between runtime events and external transport.
/// </summary>
public sealed class OutboxRelayWorker
{
    private readonly EventDispatcher _dispatcher;

    public OutboxRelayWorker(EventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task RelayBatchAsync(IReadOnlyList<EventEnvelope> envelopes)
    {
        if (envelopes.Count == 0) return;
        await _dispatcher.DispatchAsync(envelopes);
    }
}
