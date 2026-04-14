namespace Whycespace.Shared.Contracts.Economic.Vault.Account;

public sealed record ApplyRevenueCommand(
    Guid VaultAccountId,
    decimal Amount,
    string Currency);

public sealed record DebitSliceCommand(
    Guid VaultAccountId,
    VaultSliceType Slice,
    decimal Amount);

public sealed record CreditSliceCommand(
    Guid VaultAccountId,
    VaultSliceType Slice,
    decimal Amount);
