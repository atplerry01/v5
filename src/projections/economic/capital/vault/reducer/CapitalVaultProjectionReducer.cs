using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Vault;

namespace Whycespace.Projections.Economic.Capital.Vault.Reducer;

public static class CapitalVaultProjectionReducer
{
    public static CapitalVaultReadModel Apply(CapitalVaultReadModel state, VaultCreatedEventSchema e) =>
        state with
        {
            VaultId = e.AggregateId,
            OwnerId = e.OwnerId,
            Currency = e.Currency,
            TotalStored = 0m
        };

    public static CapitalVaultReadModel Apply(CapitalVaultReadModel state, VaultSliceCreatedEventSchema e) =>
        state with
        {
            VaultId = e.AggregateId,
            TotalStored = state.TotalStored + e.TotalCapacity
        };

    public static CapitalVaultReadModel Apply(CapitalVaultReadModel state, CapitalDepositedEventSchema e) =>
        state with
        {
            VaultId = e.AggregateId,
            TotalStored = e.NewVaultTotal
        };

    public static CapitalVaultReadModel Apply(CapitalVaultReadModel state, CapitalAllocatedToSliceEventSchema e) =>
        state with { VaultId = e.AggregateId };

    public static CapitalVaultReadModel Apply(CapitalVaultReadModel state, CapitalReleasedFromSliceEventSchema e) =>
        state with { VaultId = e.AggregateId };

    public static CapitalVaultReadModel Apply(CapitalVaultReadModel state, CapitalWithdrawnEventSchema e) =>
        state with
        {
            VaultId = e.AggregateId,
            TotalStored = e.NewVaultTotal
        };
}
