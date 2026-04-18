using Whycespace.Projections.Economic.Ledger.Journal.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Ledger.Journal;

/// <summary>
/// Phase 8 B2 — projects <see cref="JournalCompensationCreatedEventSchema"/>
/// into the compensation-scoped journal read model. Inline execution
/// policy mirrors the rest of the economic projection ring.
/// </summary>
public sealed class JournalProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<JournalCompensationCreatedEventSchema>
{
    private readonly PostgresProjectionStore<JournalReadModel> _store;

    public JournalProjectionHandler(PostgresProjectionStore<JournalReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            JournalCompensationCreatedEventSchema e => Project(e, envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"JournalProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(JournalCompensationCreatedEventSchema e, CancellationToken ct = default)
        => Project(e, envelope: null, ct);

    private async Task Project(
        JournalCompensationCreatedEventSchema e,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(e.AggregateId, ct) ??
                    new JournalReadModel { JournalId = e.AggregateId };
        state = JournalProjectionReducer.Apply(state, e);
        await _store.UpsertAsync(
            e.AggregateId,
            state,
            "JournalCompensationCreatedEvent",
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
