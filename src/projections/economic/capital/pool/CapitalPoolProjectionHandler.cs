using Whycespace.Projections.Economic.Capital.Pool.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Pool;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Pool;

public sealed class CapitalPoolProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<PoolCreatedEventSchema>,
    IProjectionHandler<CapitalAggregatedEventSchema>,
    IProjectionHandler<CapitalReducedEventSchema>
{
    private readonly PostgresProjectionStore<CapitalPoolReadModel> _store;

    public CapitalPoolProjectionHandler(PostgresProjectionStore<CapitalPoolReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            PoolCreatedEventSchema e => Project(e.AggregateId, s => CapitalPoolProjectionReducer.Apply(s, e), "PoolCreatedEvent", envelope, cancellationToken),
            CapitalAggregatedEventSchema e => Project(e.AggregateId, s => CapitalPoolProjectionReducer.Apply(s, e), "CapitalAggregatedEvent", envelope, cancellationToken),
            CapitalReducedEventSchema e => Project(e.AggregateId, s => CapitalPoolProjectionReducer.Apply(s, e), "CapitalReducedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalPoolProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(PoolCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalPoolProjectionReducer.Apply(s, e), "PoolCreatedEvent", null, ct);

    public Task HandleAsync(CapitalAggregatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalPoolProjectionReducer.Apply(s, e), "CapitalAggregatedEvent", null, ct);

    public Task HandleAsync(CapitalReducedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalPoolProjectionReducer.Apply(s, e), "CapitalReducedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalPoolReadModel, CapitalPoolReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalPoolReadModel { PoolId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
