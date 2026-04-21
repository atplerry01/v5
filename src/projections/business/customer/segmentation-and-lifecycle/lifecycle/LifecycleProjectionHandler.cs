using Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Lifecycle.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Customer.SegmentationAndLifecycle.Lifecycle;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class LifecycleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LifecycleStartedEventSchema>,
    IProjectionHandler<LifecycleStageChangedEventSchema>,
    IProjectionHandler<LifecycleClosedEventSchema>
{
    private readonly PostgresProjectionStore<LifecycleReadModel> _store;

    public LifecycleProjectionHandler(PostgresProjectionStore<LifecycleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LifecycleStartedEventSchema e      => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e), "LifecycleStartedEvent",      envelope, cancellationToken),
            LifecycleStageChangedEventSchema e => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e), "LifecycleStageChangedEvent", envelope, cancellationToken),
            LifecycleClosedEventSchema e       => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e), "LifecycleClosedEvent",       envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LifecycleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(LifecycleStartedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e), "LifecycleStartedEvent", null, ct);

    public Task HandleAsync(LifecycleStageChangedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e), "LifecycleStageChangedEvent", null, ct);

    public Task HandleAsync(LifecycleClosedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e), "LifecycleClosedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<LifecycleReadModel, LifecycleReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new LifecycleReadModel { LifecycleId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
