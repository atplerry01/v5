using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Structure.TopologyDefinition.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Structure.TopologyDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Structure.TopologyDefinition;

namespace Whycespace.Projections.Structural.Structure.TopologyDefinition;

public sealed class TopologyDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TopologyDefinitionCreatedEventSchema>,
    IProjectionHandler<TopologyDefinitionActivatedEventSchema>,
    IProjectionHandler<TopologyDefinitionSuspendedEventSchema>,
    IProjectionHandler<TopologyDefinitionReactivatedEventSchema>,
    IProjectionHandler<TopologyDefinitionRetiredEventSchema>
{
    private readonly PostgresProjectionStore<TopologyDefinitionReadModel> _store;

    public TopologyDefinitionProjectionHandler(PostgresProjectionStore<TopologyDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            TopologyDefinitionCreatedEventSchema e => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyDefinitionCreatedEvent", envelope, cancellationToken),
            TopologyDefinitionActivatedEventSchema e => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyDefinitionActivatedEvent", envelope, cancellationToken),
            TopologyDefinitionSuspendedEventSchema e => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyDefinitionSuspendedEvent", envelope, cancellationToken),
            TopologyDefinitionReactivatedEventSchema e => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyDefinitionReactivatedEvent", envelope, cancellationToken),
            TopologyDefinitionRetiredEventSchema e => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TopologyDefinitionRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TopologyDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(TopologyDefinitionCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyDefinitionCreatedEvent", null, ct);
    public Task HandleAsync(TopologyDefinitionActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyDefinitionActivatedEvent", null, ct);
    public Task HandleAsync(TopologyDefinitionSuspendedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyDefinitionSuspendedEvent", null, ct);
    public Task HandleAsync(TopologyDefinitionReactivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyDefinitionReactivatedEvent", null, ct);
    public Task HandleAsync(TopologyDefinitionRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TopologyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TopologyDefinitionRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<TopologyDefinitionReadModel, TopologyDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new TopologyDefinitionReadModel { TopologyDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
