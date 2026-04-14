using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;

namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Transaction;

public sealed record TransactionInitiatedEventSchema(
    Guid AggregateId,
    string Kind,
    IReadOnlyList<TransactionReferenceDto> References,
    DateTimeOffset InitiatedAt);

public sealed record TransactionCommittedEventSchema(
    Guid AggregateId,
    string Kind,
    IReadOnlyList<TransactionReferenceDto> References,
    DateTimeOffset CommittedAt);

public sealed record TransactionFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);
