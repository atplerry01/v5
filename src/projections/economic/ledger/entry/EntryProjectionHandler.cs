using Whycespace.Projections.Economic.Ledger.Entry.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Ledger.Entry;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Entry;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Entry;

public sealed class EntryProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<LedgerEntryRecordedEventSchema>
{
    private readonly PostgresProjectionStore<EntryReadModel> _store;

    public EntryProjectionHandler(PostgresProjectionStore<EntryReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            LedgerEntryRecordedEventSchema e => Project(e, envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"EntryProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(LedgerEntryRecordedEventSchema e, CancellationToken ct = default)
        => await Project(e, Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(LedgerEntryRecordedEventSchema e, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new EntryReadModel { EntryId = e.AggregateId };
        state = EntryProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(e.AggregateId, state, "LedgerEntryRecordedEvent", eventId, eventVersion, correlationId, ct);
    }
}
