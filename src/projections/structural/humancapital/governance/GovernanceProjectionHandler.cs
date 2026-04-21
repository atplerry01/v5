using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Governance.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Governance;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Governance;

namespace Whycespace.Projections.Structural.Humancapital.Governance;

public sealed class GovernanceProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<GovernanceCreatedEventSchema>
{
    private readonly PostgresProjectionStore<GovernanceReadModel> _store;

    public GovernanceProjectionHandler(PostgresProjectionStore<GovernanceReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            GovernanceCreatedEventSchema e => Project(e.AggregateId, s => GovernanceProjectionReducer.Apply(s, e, envelope.Timestamp), "GovernanceCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"GovernanceProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(GovernanceCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => GovernanceProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "GovernanceCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<GovernanceReadModel, GovernanceReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new GovernanceReadModel { GovernanceId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
