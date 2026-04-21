using Whycespace.Projections.Business.Order.OrderChange.Cancellation.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Cancellation;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Order.OrderChange.Cancellation;

public sealed class CancellationProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CancellationRequestedEventSchema>,
    IProjectionHandler<CancellationConfirmedEventSchema>,
    IProjectionHandler<CancellationRejectedEventSchema>
{
    private readonly PostgresProjectionStore<CancellationReadModel> _store;

    public CancellationProjectionHandler(PostgresProjectionStore<CancellationReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CancellationRequestedEventSchema e => Project(e.AggregateId, s => CancellationProjectionReducer.Apply(s, e), "CancellationRequestedEvent", envelope, cancellationToken),
            CancellationConfirmedEventSchema e => Project(e.AggregateId, s => CancellationProjectionReducer.Apply(s, e), "CancellationConfirmedEvent", envelope, cancellationToken),
            CancellationRejectedEventSchema  e => Project(e.AggregateId, s => CancellationProjectionReducer.Apply(s, e), "CancellationRejectedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CancellationProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CancellationRequestedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CancellationProjectionReducer.Apply(s, e), "CancellationRequestedEvent", null, ct);

    public Task HandleAsync(CancellationConfirmedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CancellationProjectionReducer.Apply(s, e), "CancellationConfirmedEvent", null, ct);

    public Task HandleAsync(CancellationRejectedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CancellationProjectionReducer.Apply(s, e), "CancellationRejectedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CancellationReadModel, CancellationReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CancellationReadModel { CancellationId = aggregateId };
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
