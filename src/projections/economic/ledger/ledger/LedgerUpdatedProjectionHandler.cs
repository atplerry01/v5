using Whycespace.Projections.Economic.Ledger.Ledger.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Ledger;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Ledger;

public sealed class LedgerUpdatedProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LedgerOpenedEventSchema>,
    IProjectionHandler<LedgerUpdatedEventSchema>,
    IProjectionHandler<JournalEntryRecordedEventSchema>
{
    private readonly PostgresProjectionStore<LedgerReadModel> _store;

    public LedgerUpdatedProjectionHandler(PostgresProjectionStore<LedgerReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LedgerOpenedEventSchema opened =>
                ProjectOpened(opened, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            LedgerUpdatedEventSchema ledger =>
                ProjectLedger(ledger, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            JournalEntryRecordedEventSchema entry =>
                ProjectEntry(entry, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"LedgerUpdatedProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(LedgerOpenedEventSchema e, CancellationToken ct = default)
        => await ProjectOpened(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(LedgerUpdatedEventSchema e, CancellationToken ct = default)
        => await ProjectLedger(e, Guid.Empty, 0, Guid.Empty, ct);

    public async Task HandleAsync(JournalEntryRecordedEventSchema e, CancellationToken ct = default)
        => await ProjectEntry(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task ProjectOpened(
        LedgerOpenedEventSchema e,
        Guid eventId,
        long eventVersion,
        Guid correlationId,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new LedgerReadModel { LedgerId = e.AggregateId };
        state = LedgerProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(
            e.AggregateId, state, "LedgerOpenedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task ProjectLedger(
        LedgerUpdatedEventSchema e,
        Guid eventId,
        long eventVersion,
        Guid correlationId,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new LedgerReadModel { LedgerId = e.AggregateId };
        state = LedgerProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(
            e.AggregateId, state, "LedgerUpdatedEvent", eventId, eventVersion, correlationId, ct);
    }

    private async Task ProjectEntry(
        JournalEntryRecordedEventSchema e,
        Guid eventId,
        long eventVersion,
        Guid correlationId,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new LedgerReadModel { LedgerId = e.AggregateId };
        state = LedgerProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(
            e.AggregateId, state, "JournalEntryRecordedEvent", eventId, eventVersion, correlationId, ct);
    }
}
