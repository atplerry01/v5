using Whycespace.Projections.Shared;
using Whycespace.Projections.Structural.Humancapital.Incentive.Reducer;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Incentive;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Structural.Humancapital.Incentive;

namespace Whycespace.Projections.Structural.Humancapital.Incentive;

public sealed class IncentiveProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<IncentiveCreatedEventSchema>
{
    private readonly PostgresProjectionStore<IncentiveReadModel> _store;

    public IncentiveProjectionHandler(PostgresProjectionStore<IncentiveReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default) =>
        envelope.Payload switch
        {
            IncentiveCreatedEventSchema e => Project(e.AggregateId, s => IncentiveProjectionReducer.Apply(s, e, envelope.Timestamp), "IncentiveCreatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"IncentiveProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };

    public Task HandleAsync(IncentiveCreatedEventSchema e, CancellationToken ct = default) => Project(e.AggregateId, s => IncentiveProjectionReducer.Apply(s, e, DateTimeOffset.UtcNow), "IncentiveCreatedEvent", null, ct);

    private async Task Project(Guid aggregateId, Func<IncentiveReadModel, IncentiveReadModel> reduce, string eventTypeName, IEventEnvelope? envelope, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new IncentiveReadModel { IncentiveId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventTypeName, envelope?.EventId ?? Guid.Empty, envelope?.SequenceNumber ?? 0, envelope?.CorrelationId ?? Guid.Empty, ct);
    }
}
