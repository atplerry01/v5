using Whycespace.Projections.Economic.Ledger.Treasury.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Treasury;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Treasury;

public sealed class TreasuryProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TreasuryCreatedEventSchema>,
    IProjectionHandler<TreasuryFundAllocatedEventSchema>,
    IProjectionHandler<TreasuryFundReleasedEventSchema>
{
    private readonly PostgresProjectionStore<TreasuryReadModel> _store;

    public TreasuryProjectionHandler(PostgresProjectionStore<TreasuryReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            TreasuryCreatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TreasuryFundAllocatedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TreasuryFundReleasedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TreasuryProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(TreasuryCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TreasuryFundAllocatedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(TreasuryFundReleasedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(TreasuryCreatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new TreasuryReadModel { TreasuryId = e.AggregateId };
        state = TreasuryProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TreasuryCreatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TreasuryFundAllocatedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new TreasuryReadModel { TreasuryId = e.AggregateId };
        state = TreasuryProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TreasuryFundAllocatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task Project(TreasuryFundReleasedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new TreasuryReadModel { TreasuryId = e.AggregateId };
        state = TreasuryProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "TreasuryFundReleasedEvent", eventId, eventVersion, correlationId, ct);
    }
}
