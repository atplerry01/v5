namespace Whycespace.Domain.EconomicSystem.Transaction.Wallet;

public static class WalletErrors
{
    public static DomainException AlreadyFrozen(Guid walletId) =>
        new("WALLET_ALREADY_FROZEN", $"WalletAggregate {walletId} is already frozen.");

    public static DomainException NotActive(Guid walletId) =>
        new("WALLET_NOT_ACTIVE", $"WalletAggregate {walletId} is not in an active state.");

    public static DomainException InsufficientBalance(Guid walletId) =>
        new("WALLET_INSUFFICIENT_BALANCE", $"WalletAggregate {walletId} has insufficient balance.");

    public static DomainException IdentityMismatch(Guid walletId, Guid identityId) =>
        new("WALLET_IDENTITY_MISMATCH", $"WalletAggregate {walletId} does not belong to identity {identityId}.");
}
