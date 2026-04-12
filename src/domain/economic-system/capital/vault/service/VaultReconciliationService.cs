using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class VaultReconciliationService
{
    /// <summary>
    /// Validates that vault total equals the sum of all slice capacities.
    /// This is the cross-entity invariant: no capital can exist outside a slice.
    /// </summary>
    public bool ValidateSliceConsistency(VaultAggregate vault)
    {
        var sliceCapacitySum = 0m;
        foreach (var slice in vault.Slices)
        {
            sliceCapacitySum += slice.TotalCapacity.Value;
        }

        return vault.TotalStored.Value == sliceCapacitySum;
    }

    /// <summary>
    /// Validates that vault total stored does not exceed the corresponding account total.
    /// </summary>
    public bool ValidateVaultAccountAlignment(VaultAggregate vault, Amount accountTotal) =>
        vault.TotalStored.Value <= accountTotal.Value;
}
