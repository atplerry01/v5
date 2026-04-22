using Whycespace.Projections.Platform.Schema.SchemaDefinition.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Schema.SchemaDefinition;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Schema.SchemaDefinition;

public sealed class SchemaDefinitionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SchemaDefinitionDraftedEventSchema>,
    IProjectionHandler<SchemaDefinitionPublishedEventSchema>,
    IProjectionHandler<SchemaDefinitionDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<SchemaDefinitionReadModel> _store;

    public SchemaDefinitionProjectionHandler(PostgresProjectionStore<SchemaDefinitionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SchemaDefinitionDraftedEventSchema e => Project(e.AggregateId, s => SchemaDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "SchemaDefinitionDraftedEvent", envelope, cancellationToken),
            SchemaDefinitionPublishedEventSchema e => Project(e.AggregateId, s => SchemaDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "SchemaDefinitionPublishedEvent", envelope, cancellationToken),
            SchemaDefinitionDeprecatedEventSchema e => Project(e.AggregateId, s => SchemaDefinitionProjectionReducer.Apply(s, e, envelope.Timestamp), "SchemaDefinitionDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"SchemaDefinitionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SchemaDefinitionDraftedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SchemaDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SchemaDefinitionDraftedEvent", null, ct);
    public Task HandleAsync(SchemaDefinitionPublishedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SchemaDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SchemaDefinitionPublishedEvent", null, ct);
    public Task HandleAsync(SchemaDefinitionDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SchemaDefinitionProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SchemaDefinitionDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SchemaDefinitionReadModel, SchemaDefinitionReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SchemaDefinitionReadModel { SchemaDefinitionId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
