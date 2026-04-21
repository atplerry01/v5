using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Subcluster.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Subcluster;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

namespace Whycespace.Projections.Structural.Cluster.Subcluster;

public sealed class SubclusterProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SubclusterDefinedEventSchema>,
    IProjectionHandler<SubclusterAttachedEventSchema>,
    IProjectionHandler<SubclusterBindingValidatedEventSchema>,
    IProjectionHandler<SubclusterActivatedEventSchema>,
    IProjectionHandler<SubclusterSuspendedEventSchema>,
    IProjectionHandler<SubclusterReactivatedEventSchema>,
    IProjectionHandler<SubclusterArchivedEventSchema>,
    IProjectionHandler<SubclusterRetiredEventSchema>
{
    private readonly PostgresProjectionStore<SubclusterReadModel> _store;

    public SubclusterProjectionHandler(PostgresProjectionStore<SubclusterReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SubclusterDefinedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterDefinedEvent", envelope, cancellationToken),
            SubclusterAttachedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterAttachedEvent", envelope, cancellationToken),
            SubclusterBindingValidatedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterBindingValidatedEvent", envelope, cancellationToken),
            SubclusterActivatedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterActivatedEvent", envelope, cancellationToken),
            SubclusterSuspendedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterSuspendedEvent", envelope, cancellationToken),
            SubclusterReactivatedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterReactivatedEvent", envelope, cancellationToken),
            SubclusterArchivedEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterArchivedEvent", envelope, cancellationToken),
            SubclusterRetiredEventSchema e => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, envelope.Timestamp), "SubclusterRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SubclusterProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SubclusterDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterDefinedEvent", null, ct);
    public Task HandleAsync(SubclusterAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterAttachedEvent", null, ct);
    public Task HandleAsync(SubclusterBindingValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterBindingValidatedEvent", null, ct);
    public Task HandleAsync(SubclusterActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterActivatedEvent", null, ct);
    public Task HandleAsync(SubclusterSuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterSuspendedEvent", null, ct);
    public Task HandleAsync(SubclusterReactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterReactivatedEvent", null, ct);
    public Task HandleAsync(SubclusterArchivedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterArchivedEvent", null, ct);
    public Task HandleAsync(SubclusterRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SubclusterProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SubclusterRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SubclusterReadModel, SubclusterReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SubclusterReadModel { SubclusterId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
