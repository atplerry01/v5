namespace Whycespace.Engines.T2E.Economic.Transaction.Wallet;

public record WalletCommand(string Action, string EntityId, object Payload);

public sealed record CreateWalletCommand(string WalletId, string IdentityId, string CurrencyCode)
    : WalletCommand("Create", WalletId, null!);

public sealed record FreezeWalletCommand(string WalletId, string Reason)
    : WalletCommand("Freeze", WalletId, null!);

public sealed record UnfreezeWalletCommand(string WalletId)
    : WalletCommand("Unfreeze", WalletId, null!);
