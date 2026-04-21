using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Cluster.Spv.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Cluster.Spv;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Cluster.Spv;

namespace Whycespace.Projections.Structural.Cluster.Spv;

public sealed class SpvProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SpvCreatedEventSchema>,
    IProjectionHandler<SpvAttachedEventSchema>,
    IProjectionHandler<SpvBindingValidatedEventSchema>,
    IProjectionHandler<SpvActivatedEventSchema>,
    IProjectionHandler<SpvSuspendedEventSchema>,
    IProjectionHandler<SpvClosedEventSchema>,
    IProjectionHandler<SpvReactivatedEventSchema>,
    IProjectionHandler<SpvRetiredEventSchema>
{
    private readonly PostgresProjectionStore<SpvReadModel> _store;

    public SpvProjectionHandler(PostgresProjectionStore<SpvReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SpvCreatedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvCreatedEvent", envelope, cancellationToken),
            SpvAttachedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvAttachedEvent", envelope, cancellationToken),
            SpvBindingValidatedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvBindingValidatedEvent", envelope, cancellationToken),
            SpvActivatedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvActivatedEvent", envelope, cancellationToken),
            SpvSuspendedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvSuspendedEvent", envelope, cancellationToken),
            SpvClosedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvClosedEvent", envelope, cancellationToken),
            SpvReactivatedEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvReactivatedEvent", envelope, cancellationToken),
            SpvRetiredEventSchema e => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, envelope.Timestamp), "SpvRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"SpvProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SpvCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvCreatedEvent", null, ct);
    public Task HandleAsync(SpvAttachedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvAttachedEvent", null, ct);
    public Task HandleAsync(SpvBindingValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvBindingValidatedEvent", null, ct);
    public Task HandleAsync(SpvActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvActivatedEvent", null, ct);
    public Task HandleAsync(SpvSuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvSuspendedEvent", null, ct);
    public Task HandleAsync(SpvClosedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvClosedEvent", null, ct);
    public Task HandleAsync(SpvReactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvReactivatedEvent", null, ct);
    public Task HandleAsync(SpvRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SpvProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SpvRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SpvReadModel, SpvReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SpvReadModel { SpvId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
