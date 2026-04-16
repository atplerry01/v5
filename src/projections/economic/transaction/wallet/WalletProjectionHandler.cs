using Whycespace.Projections.Economic.Transaction.Wallet.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Transaction.Wallet;

public sealed class WalletProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<WalletCreatedEventSchema>,
    IProjectionHandler<TransactionRequestedEventSchema>
{
    private readonly PostgresProjectionStore<WalletReadModel> _store;

    public WalletProjectionHandler(PostgresProjectionStore<WalletReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            WalletCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TransactionRequestedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"WalletProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(WalletCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TransactionRequestedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(WalletCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new WalletReadModel { WalletId = e.AggregateId };
        state = WalletProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "WalletCreatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TransactionRequestedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new WalletReadModel { WalletId = e.AggregateId };
        state = WalletProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TransactionRequestedEvent", eventId, eventVersion, correlationId, ct);
    }
}
