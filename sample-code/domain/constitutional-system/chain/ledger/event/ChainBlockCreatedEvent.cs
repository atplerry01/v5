namespace Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;

public sealed record ChainBlockCreatedEvent(
    string BlockId,
    string PreviousHash,
    string CurrentHash,
    string PayloadHash,
    string PayloadType,
    string PayloadId,
    DateTimeOffset Timestamp,
    string CorrelationIdValue
) : DomainEvent;
