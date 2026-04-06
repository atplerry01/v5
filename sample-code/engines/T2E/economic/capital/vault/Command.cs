namespace Whycespace.Engines.T2E.Economic.Capital.Vault;

public record VaultCommand(string Action, string EntityId, object Payload);
public sealed record CreateVaultCommand(string Id) : VaultCommand("Create", Id, null!);
public sealed record DepositFundsCommand(string VaultId, decimal Amount, string CurrencyCode) : VaultCommand("Deposit", VaultId, null!);
public sealed record WithdrawFundsCommand(string VaultId, decimal Amount, string CurrencyCode) : VaultCommand("Withdraw", VaultId, null!);
public sealed record LockFundsCommand(string VaultId, decimal Amount, string Reason) : VaultCommand("Lock", VaultId, null!);
