using Whycespace.Projections.Platform.Event.EventMetadata.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Event.EventMetadata;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Event.EventMetadata;

public sealed class EventMetadataProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EventMetadataAttachedEventSchema>
{
    private readonly PostgresProjectionStore<EventMetadataReadModel> _store;

    public EventMetadataProjectionHandler(PostgresProjectionStore<EventMetadataReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            EventMetadataAttachedEventSchema e => Project(e.AggregateId, s => EventMetadataProjectionReducer.Apply(s, e, envelope.Timestamp), "EventMetadataAttachedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"EventMetadataProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(EventMetadataAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EventMetadataProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "EventMetadataAttachedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<EventMetadataReadModel, EventMetadataReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new EventMetadataReadModel { EventMetadataId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
