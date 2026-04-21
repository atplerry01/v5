using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Structure.TypeDefinition.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Structure.TypeDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Structure.TypeDefinition;

namespace Whycespace.Projections.Structural.Structure.TypeDefinition;

public sealed class TypeDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TypeDefinitionDefinedEventSchema>,
    IProjectionHandler<TypeDefinitionActivatedEventSchema>,
    IProjectionHandler<TypeDefinitionRetiredEventSchema>
{
    private readonly PostgresProjectionStore<TypeDefinitionReadModel> _store;

    public TypeDefinitionProjectionHandler(PostgresProjectionStore<TypeDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            TypeDefinitionDefinedEventSchema e => Project(e.AggregateId, s => TypeDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TypeDefinitionDefinedEvent", envelope, cancellationToken),
            TypeDefinitionActivatedEventSchema e => Project(e.AggregateId, s => TypeDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TypeDefinitionActivatedEvent", envelope, cancellationToken),
            TypeDefinitionRetiredEventSchema e => Project(e.AggregateId, s => TypeDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "TypeDefinitionRetiredEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TypeDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(TypeDefinitionDefinedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TypeDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TypeDefinitionDefinedEvent", null, ct);
    public Task HandleAsync(TypeDefinitionActivatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TypeDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TypeDefinitionActivatedEvent", null, ct);
    public Task HandleAsync(TypeDefinitionRetiredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => TypeDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "TypeDefinitionRetiredEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<TypeDefinitionReadModel, TypeDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new TypeDefinitionReadModel { TypeDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
