using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Lifecycle.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Lifecycle;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Lifecycle;

namespace Whycespace.Projections.Structural.Cluster.Lifecycle;

public sealed class LifecycleProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LifecycleDefinedEventSchema>,
    IProjectionHandler<LifecycleTransitionedEventSchema>,
    IProjectionHandler<LifecycleCompletedEventSchema>
{
    private readonly PostgresProjectionStore<LifecycleReadModel> _store;

    public LifecycleProjectionHandler(PostgresProjectionStore<LifecycleReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            LifecycleDefinedEventSchema e => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e, envelope.Timestamp), "LifecycleDefinedEvent", envelope, cancellationToken),
            LifecycleTransitionedEventSchema e => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e, envelope.Timestamp), "LifecycleTransitionedEvent", envelope, cancellationToken),
            LifecycleCompletedEventSchema e => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e, envelope.Timestamp), "LifecycleCompletedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LifecycleProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(LifecycleDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "LifecycleDefinedEvent", null, ct);
    public Task HandleAsync(LifecycleTransitionedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "LifecycleTransitionedEvent", null, ct);
    public Task HandleAsync(LifecycleCompletedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => LifecycleProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "LifecycleCompletedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<LifecycleReadModel, LifecycleReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new LifecycleReadModel { LifecycleId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
