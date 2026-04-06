using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Event.EventEnvelope;

public sealed class EventEnvelopeProjectionHandler
{
    public string ProjectionName => "whyce.core.event.event-envelope";

    public string[] EventTypes =>
    [
        "whyce.core.event.event-envelope.created",
        "whyce.core.event.event-envelope.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEventEnvelopeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EventEnvelopeReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
