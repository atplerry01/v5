using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Participant.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Participant;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Participant;

namespace Whycespace.Projections.Structural.Humancapital.Participant;

public sealed class ParticipantProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ParticipantRegisteredEventSchema>,
    IProjectionHandler<ParticipantPlacedEventSchema>
{
    private readonly PostgresProjectionStore<ParticipantReadModel> _store;

    public ParticipantProjectionHandler(PostgresProjectionStore<ParticipantReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ParticipantRegisteredEventSchema e => Project(e.AggregateId, s => ParticipantProjectionReducer.Apply(s, e, envelope.Timestamp), "ParticipantRegisteredEvent", envelope, cancellationToken),
            ParticipantPlacedEventSchema e => Project(e.AggregateId, s => ParticipantProjectionReducer.Apply(s, e, envelope.Timestamp), "ParticipantPlacedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ParticipantProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ParticipantRegisteredEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ParticipantProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ParticipantRegisteredEvent", null, ct);
    public Task HandleAsync(ParticipantPlacedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ParticipantProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ParticipantPlacedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ParticipantReadModel, ParticipantReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ParticipantReadModel { ParticipantId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
