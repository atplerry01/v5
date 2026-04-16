using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Transaction.Wallet;

public sealed record CreateWalletCommand(
    Guid WalletId,
    Guid OwnerId,
    Guid AccountId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => WalletId;
}

public sealed record RequestWalletTransactionCommand(
    Guid WalletId,
    Guid DestinationAccountId,
    decimal Amount,
    string Currency,
    DateTimeOffset RequestedAt) : IHasAggregateId
{
    public Guid AggregateId => WalletId;
}
