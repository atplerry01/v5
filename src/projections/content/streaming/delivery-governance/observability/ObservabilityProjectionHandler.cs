using Whycespace.Projections.Content.Streaming.DeliveryGovernance.Observability.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Observability;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Content.Streaming.DeliveryGovernance.Observability;

public sealed class ObservabilityProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ObservabilityCapturedEventSchema>,
    IProjectionHandler<ObservabilityUpdatedEventSchema>,
    IProjectionHandler<ObservabilityFinalizedEventSchema>,
    IProjectionHandler<ObservabilityArchivedEventSchema>
{
    private readonly PostgresProjectionStore<ObservabilityReadModel> _store;

    public ObservabilityProjectionHandler(PostgresProjectionStore<ObservabilityReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ObservabilityCapturedEventSchema e => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityCapturedEvent", envelope, cancellationToken),
            ObservabilityUpdatedEventSchema e => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityUpdatedEvent", envelope, cancellationToken),
            ObservabilityFinalizedEventSchema e => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityFinalizedEvent", envelope, cancellationToken),
            ObservabilityArchivedEventSchema e => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityArchivedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ObservabilityProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ObservabilityCapturedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityCapturedEvent", null, ct);
    public Task HandleAsync(ObservabilityUpdatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityUpdatedEvent", null, ct);
    public Task HandleAsync(ObservabilityFinalizedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityFinalizedEvent", null, ct);
    public Task HandleAsync(ObservabilityArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ObservabilityProjectionReducer.Apply(s, e), "ObservabilityArchivedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ObservabilityReadModel, ObservabilityReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ObservabilityReadModel { ObservabilityId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
