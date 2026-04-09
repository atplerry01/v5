using Whyce.Shared.Contracts.Infrastructure.Messaging;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Outbox Service — bridge between Event Fabric and external systems.
/// Events are enqueued to the outbox for eventual relay to infrastructure adapters (e.g., Kafka).
/// The outbox is NOT the primary messaging layer — it is a durability bridge.
/// Only invoked by the Event Fabric orchestrator.
/// </summary>
public sealed class OutboxService
{
    private readonly IOutbox _outbox;

    public OutboxService(IOutbox outbox)
    {
        _outbox = outbox;
    }

    // phase1.5-S5.2.3 / TC-5 (POSTGRES-CT-THREAD-01): forwards the
    // request/host-shutdown CancellationToken from EventFabric down to
    // IOutbox so the underlying Postgres adapter ExecuteNonQueryAsync
    // calls honor cancellation.
    public async Task EnqueueAsync(
        Guid correlationId,
        IReadOnlyList<object> domainEvents,
        string topic,
        CancellationToken cancellationToken = default)
    {
        if (domainEvents.Count == 0) return;
        await _outbox.EnqueueAsync(correlationId, domainEvents, topic, cancellationToken);
    }
}
