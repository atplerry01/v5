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

    public async Task EnqueueAsync(Guid correlationId, IReadOnlyList<object> domainEvents)
    {
        if (domainEvents.Count == 0) return;
        await _outbox.EnqueueAsync(correlationId, domainEvents);
    }
}
