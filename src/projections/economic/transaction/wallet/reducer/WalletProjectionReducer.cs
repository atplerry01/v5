using Whycespace.Shared.Contracts.Economic.Transaction.Wallet;
using Whycespace.Shared.Contracts.Events.Economic.Transaction.Wallet;

namespace Whycespace.Projections.Economic.Transaction.Wallet.Reducer;

public static class WalletProjectionReducer
{
    public static WalletReadModel Apply(WalletReadModel state, WalletCreatedEventSchema e) =>
        state with
        {
            WalletId = e.AggregateId,
            OwnerId = e.OwnerId,
            AccountId = e.AccountId,
            Status = "Active",
            CreatedAt = e.CreatedAt
        };

    public static WalletReadModel Apply(WalletReadModel state, TransactionRequestedEventSchema e) =>
        state with
        {
            WalletId = e.AggregateId,
            LastTransactionRequestedAt = e.RequestedAt
        };
}
