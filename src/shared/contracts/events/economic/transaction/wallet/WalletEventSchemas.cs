namespace Whycespace.Shared.Contracts.Events.Economic.Transaction.Wallet;

public sealed record WalletCreatedEventSchema(
    Guid AggregateId,
    Guid OwnerId,
    Guid AccountId,
    DateTimeOffset CreatedAt);

public sealed record TransactionRequestedEventSchema(
    Guid AggregateId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset RequestedAt);
