namespace Whycespace.Shared.Contracts.Economic.Vault.Account;

public sealed record VaultAccountReadModel
{
    public Guid VaultAccountId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal TotalBalance { get; init; }
    public decimal FreeBalance { get; init; }
    public decimal LockedBalance { get; init; }
    public decimal InvestedBalance { get; init; }
}
