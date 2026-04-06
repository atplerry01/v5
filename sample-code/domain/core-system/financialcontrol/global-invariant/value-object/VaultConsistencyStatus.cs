namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

/// <summary>
/// Represents the consistency status of vault balances across the system.
/// </summary>
public sealed record VaultConsistencyStatus
{
    public bool IsConsistent { get; }
    public int VaultsVerified { get; }
    public int DiscrepanciesFound { get; }

    private VaultConsistencyStatus(bool isConsistent, int vaultsVerified, int discrepanciesFound)
    {
        IsConsistent = isConsistent;
        VaultsVerified = vaultsVerified;
        DiscrepanciesFound = discrepanciesFound;
    }

    public static VaultConsistencyStatus Consistent(int vaultsVerified) =>
        new(true, vaultsVerified, 0);

    public static VaultConsistencyStatus Inconsistent(int vaultsVerified, int discrepanciesFound) =>
        discrepanciesFound <= 0
            ? throw new ArgumentOutOfRangeException(nameof(discrepanciesFound), "Discrepancies must be positive for inconsistent status.")
            : new(false, vaultsVerified, discrepanciesFound);
}
