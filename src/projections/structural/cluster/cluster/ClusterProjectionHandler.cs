using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Cluster.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Cluster;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

namespace Whycespace.Projections.Structural.Cluster.Cluster;

public sealed class ClusterProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ClusterDefinedEventSchema>,
    IProjectionHandler<ClusterActivatedEventSchema>,
    IProjectionHandler<ClusterArchivedEventSchema>,
    IProjectionHandler<ClusterAuthorityBoundEventSchema>,
    IProjectionHandler<ClusterAuthorityReleasedEventSchema>,
    IProjectionHandler<ClusterAdministrationBoundEventSchema>,
    IProjectionHandler<ClusterAdministrationReleasedEventSchema>
{
    private readonly PostgresProjectionStore<ClusterReadModel> _store;

    public ClusterProjectionHandler(PostgresProjectionStore<ClusterReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ClusterDefinedEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterDefinedEvent", envelope, cancellationToken),
            ClusterActivatedEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterActivatedEvent", envelope, cancellationToken),
            ClusterArchivedEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterArchivedEvent", envelope, cancellationToken),
            ClusterAuthorityBoundEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterAuthorityBoundEvent", envelope, cancellationToken),
            ClusterAuthorityReleasedEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterAuthorityReleasedEvent", envelope, cancellationToken),
            ClusterAdministrationBoundEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterAdministrationBoundEvent", envelope, cancellationToken),
            ClusterAdministrationReleasedEventSchema e => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, envelope.Timestamp), "ClusterAdministrationReleasedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ClusterProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ClusterDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterDefinedEvent", null, ct);
    public Task HandleAsync(ClusterActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterActivatedEvent", null, ct);
    public Task HandleAsync(ClusterArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterArchivedEvent", null, ct);
    public Task HandleAsync(ClusterAuthorityBoundEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterAuthorityBoundEvent", null, ct);
    public Task HandleAsync(ClusterAuthorityReleasedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterAuthorityReleasedEvent", null, ct);
    public Task HandleAsync(ClusterAdministrationBoundEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterAdministrationBoundEvent", null, ct);
    public Task HandleAsync(ClusterAdministrationReleasedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ClusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ClusterAdministrationReleasedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ClusterReadModel, ClusterReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ClusterReadModel { ClusterId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
