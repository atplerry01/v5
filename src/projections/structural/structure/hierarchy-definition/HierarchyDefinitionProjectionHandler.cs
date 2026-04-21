using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Structure.HierarchyDefinition.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Structure.HierarchyDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Structure.HierarchyDefinition;

namespace Whycespace.Projections.Structural.Structure.HierarchyDefinition;

public sealed class HierarchyDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<HierarchyDefinitionDefinedEventSchema>,
    IProjectionHandler<HierarchyDefinitionValidatedEventSchema>,
    IProjectionHandler<HierarchyDefinitionLockedEventSchema>
{
    private readonly PostgresProjectionStore<HierarchyDefinitionReadModel> _store;

    public HierarchyDefinitionProjectionHandler(PostgresProjectionStore<HierarchyDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            HierarchyDefinitionDefinedEventSchema e => Project(e.AggregateId, s => HierarchyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "HierarchyDefinitionDefinedEvent", envelope, cancellationToken),
            HierarchyDefinitionValidatedEventSchema e => Project(e.AggregateId, s => HierarchyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "HierarchyDefinitionValidatedEvent", envelope, cancellationToken),
            HierarchyDefinitionLockedEventSchema e => Project(e.AggregateId, s => HierarchyDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "HierarchyDefinitionLockedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"HierarchyDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(HierarchyDefinitionDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => HierarchyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "HierarchyDefinitionDefinedEvent", null, ct);
    public Task HandleAsync(HierarchyDefinitionValidatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => HierarchyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "HierarchyDefinitionValidatedEvent", null, ct);
    public Task HandleAsync(HierarchyDefinitionLockedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => HierarchyDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "HierarchyDefinitionLockedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<HierarchyDefinitionReadModel, HierarchyDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new HierarchyDefinitionReadModel { HierarchyDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
