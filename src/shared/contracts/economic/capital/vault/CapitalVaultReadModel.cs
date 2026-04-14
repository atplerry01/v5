namespace Whycespace.Shared.Contracts.Economic.Capital.Vault;

public sealed record CapitalVaultReadModel
{
    public Guid VaultId { get; init; }
    public Guid OwnerId { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal TotalStored { get; init; }
}
