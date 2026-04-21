using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Topology.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Topology;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Topology;

namespace Whycespace.Projections.Structural.Cluster.Topology;

public sealed class TopologyProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TopologyDefinedEventSchema>,
    IProjectionHandler<TopologyValidatedEventSchema>,
    IProjectionHandler<TopologyLockedEventSchema>
{
    private readonly PostgresProjectionStore<TopologyReadModel> _store;

    public TopologyProjectionHandler(PostgresProjectionStore<TopologyReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            TopologyDefinedEventSchema e => Project(e.AggregateId, s => TopologyProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyDefinedEvent", envelope, cancellationToken),
            TopologyValidatedEventSchema e => Project(e.AggregateId, s => TopologyProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyValidatedEvent", envelope, cancellationToken),
            TopologyLockedEventSchema e => Project(e.AggregateId, s => TopologyProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyLockedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TopologyProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(TopologyDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyDefinedEvent", null, ct);
    public Task HandleAsync(TopologyValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyValidatedEvent", null, ct);
    public Task HandleAsync(TopologyLockedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyLockedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<TopologyReadModel, TopologyReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new TopologyReadModel { TopologyId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
