using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;

namespace Whycespace.Projections.Economic.Ledger.Journal.Reducer;

/// <summary>
/// Phase 8 B2 — reducer for the compensation-scoped
/// <see cref="JournalReadModel"/>. Pure function of (prior state, event);
/// no hidden fetches, no inference. Full replay from event 0 produces
/// the same final state as incremental replay of the same stream.
/// </summary>
public static class JournalProjectionReducer
{
    public static JournalReadModel Apply(JournalReadModel state, JournalCompensationCreatedEventSchema e) =>
        state with
        {
            JournalId = e.AggregateId,
            Kind = "Compensating",
            OriginalJournalId = e.Compensation.OriginalJournalId,
            CompensationReason = e.Compensation.Reason,
            CreatedAt = e.CreatedAt,
            LastUpdatedAt = e.CreatedAt,
        };
}
