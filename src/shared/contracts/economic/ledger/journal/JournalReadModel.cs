namespace Whycespace.Shared.Contracts.Economic.Ledger.Journal;

/// <summary>
/// Phase 8 B2 — journal-level read model. Mirrors the aggregate's Kind
/// and CompensationReference so queries of the form "give me every
/// compensating journal that reverses X" resolve without replaying the
/// event stream.
///
/// The projection is aggregate-id-scoped (JournalId) and populated by:
///   - <c>JournalCompensationCreatedEvent</c> — creates a Compensating row
///     with the original journal id and reason stamped from the aggregate.
/// Standard journals are not materialised as rows here — their metadata
/// is already captured by the ledger projection (posted journal ids,
/// per-account balances). This projection is compensation-specific on
/// purpose.
/// </summary>
public sealed record JournalReadModel
{
    public Guid JournalId { get; init; }

    /// <summary>"Standard" | "Compensating".</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>
    /// Non-empty only when <see cref="Kind"/> == "Compensating". Points at
    /// the original Posted journal this one reverses.
    /// </summary>
    public Guid? OriginalJournalId { get; init; }

    public string CompensationReason { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
