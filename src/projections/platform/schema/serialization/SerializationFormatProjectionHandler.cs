using Whycespace.Projections.Platform.Schema.Serialization.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Platform.Schema.Serialization;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Platform.Schema.Serialization;

public sealed class SerializationFormatProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<SerializationFormatRegisteredEventSchema>,
    IProjectionHandler<SerializationFormatDeprecatedEventSchema>
{
    private readonly PostgresProjectionStore<SerializationFormatReadModel> _store;

    public SerializationFormatProjectionHandler(PostgresProjectionStore<SerializationFormatReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            SerializationFormatRegisteredEventSchema e => Project(e.AggregateId, s => SerializationFormatProjectionReducer.Apply(s, e, envelope.Timestamp), "SerializationFormatRegisteredEvent", envelope, cancellationToken),
            SerializationFormatDeprecatedEventSchema e => Project(e.AggregateId, s => SerializationFormatProjectionReducer.Apply(s, e, envelope.Timestamp), "SerializationFormatDeprecatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException($"SerializationFormatProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(SerializationFormatRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SerializationFormatProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SerializationFormatRegisteredEvent", null, ct);
    public Task HandleAsync(SerializationFormatDeprecatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => SerializationFormatProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "SerializationFormatDeprecatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<SerializationFormatReadModel, SerializationFormatReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new SerializationFormatReadModel { SerializationFormatId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
