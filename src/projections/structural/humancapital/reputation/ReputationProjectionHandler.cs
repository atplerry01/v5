using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Reputation.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Reputation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Reputation;

namespace Whycespace.Projections.Structural.Humancapital.Reputation;

public sealed class ReputationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReputationCreatedEventSchema>
{
    private readonly PostgresProjectionStore<ReputationReadModel> _store;

    public ReputationProjectionHandler(PostgresProjectionStore<ReputationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            ReputationCreatedEventSchema e => Project(e.AggregateId, s => ReputationProjectionReducer.Apply(s, e, envelope.Timestamp), "ReputationCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ReputationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(ReputationCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => ReputationProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "ReputationCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<ReputationReadModel, ReputationReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new ReputationReadModel { ReputationId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
