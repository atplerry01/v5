using Whycespace.Projections.Constitutional.Chain.Ledger.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Constitutional.Chain;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Chain.Ledger;

public sealed class LedgerProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LedgerOpenedEventSchema>,
    IProjectionHandler<LedgerSealedEventSchema>
{
    private readonly PostgresProjectionStore<LedgerReadModel> _store;

    public LedgerProjectionHandler(PostgresProjectionStore<LedgerReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LedgerOpenedEventSchema e => Project(e.AggregateId, s => LedgerProjectionReducer.Apply(s, e), "LedgerOpenedEvent", envelope, cancellationToken),
            LedgerSealedEventSchema e => Project(e.AggregateId, s => LedgerProjectionReducer.Apply(s, e), "LedgerSealedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LedgerProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(LedgerOpenedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LedgerProjectionReducer.Apply(s, e), "LedgerOpenedEvent", null, ct);

    public Task HandleAsync(LedgerSealedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => LedgerProjectionReducer.Apply(s, e), "LedgerSealedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<LedgerReadModel, LedgerReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new LedgerReadModel { LedgerId = aggregateId };
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
