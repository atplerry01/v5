namespace Whycespace.Shared.Contracts.Economic.Vault.Account;

// Contract-layer mirror of the domain's SliceType enum. Shared contracts
// cannot reference the domain assembly, so the enum is duplicated here.
// Integer values MUST match Whycespace.Domain.EconomicSystem.Vault.Slice.SliceType.
public enum VaultSliceType
{
    Slice1 = 1,
    Slice2 = 2,
    Slice3 = 3,
    Slice4 = 4
}
