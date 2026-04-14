namespace Whycespace.Shared.Contracts.Economic.Transaction.Transaction;

public sealed record TransactionReferenceDto(string Kind, Guid Id);

public sealed record InitiateTransactionCommand(
    Guid TransactionId,
    string Kind,
    IReadOnlyList<TransactionReferenceDto> References,
    DateTimeOffset InitiatedAt);

public sealed record CommitTransactionCommand(
    Guid TransactionId,
    DateTimeOffset CommittedAt);

public sealed record FailTransactionCommand(
    Guid TransactionId,
    string Reason,
    DateTimeOffset FailedAt);
