namespace Whycespace.Shared.Contracts.Events.Economic.Ledger.Journal;

public sealed record JournalEntryRecordedEventSchema(
    Guid AggregateId,
    Guid EntryId,
    Guid AccountId,
    decimal Amount,
    string Currency,
    string Direction);

public sealed record JournalPostedEventSchema(
    Guid AggregateId,
    decimal TotalDebit,
    decimal TotalCredit,
    string Currency);

/// <summary>
/// Phase 7 B3 / T7.4 — compensating-journal creation wire shape. Carries
/// the <see cref="CompensationReferenceDto"/> that provably links the
/// Kind=Compensating journal back to the original it reverses.
/// </summary>
public sealed record JournalCompensationCreatedEventSchema(
    Guid AggregateId,
    CompensationReferenceDto Compensation,
    DateTimeOffset CreatedAt);

/// <summary>
/// Phase 7 B3 / T7.4 — wire-safe DTO for the domain
/// <c>CompensationReference</c> value object. Carries only primitives so
/// the schema has no domain-type dependency; the mapper projects the VO
/// onto this shape, and projections / reactors consume it verbatim.
/// </summary>
public sealed record CompensationReferenceDto(
    Guid OriginalJournalId,
    string Reason);
