using Whycespace.Projections.Platform.Event.EventDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Event.EventDefinition;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Event.EventDefinition;

public sealed class EventDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<EventDefinedEventSchema>,
    IProjectionHandler<EventDefinitionDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<EventDefinitionReadModel> _store;

    public EventDefinitionProjectionHandler(PostgresProjectionStore<EventDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            EventDefinedEventSchema e => Project(e.AggregateId, s => EventDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "EventDefinedEvent", envelope, cancellationToken),
            EventDefinitionDeprecatedEventSchema e => Project(e.AggregateId, s => EventDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "EventDefinitionDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"EventDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(EventDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EventDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "EventDefinedEvent", null, ct);
    public Task HandleAsync(EventDefinitionDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => EventDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "EventDefinitionDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<EventDefinitionReadModel, EventDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new EventDefinitionReadModel { EventDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
