using Whycespace.Domain.EconomicSystem.Capital.Vault;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class DistributionVaultRoutingService
{
    public IReadOnlyList<VaultTransfer> BuildTransfers(
        IReadOnlyList<DistributionAllocation> allocations)
    {
        if (allocations == null || allocations.Count == 0)
            throw new DistributionException("No allocations to route");

        var transfers = new List<VaultTransfer>(allocations.Count);

        foreach (var allocation in allocations)
        {
            var transferId = DeterministicIdHelper.FromSeed(
                $"VaultTransfer:{allocation.RecipientId}:{allocation.Amount.Amount}:{allocation.Amount.Currency.Code}");
            transfers.Add(new VaultTransfer(
                transferId,
                allocation.RecipientId,
                allocation.Amount.Amount,
                allocation.Amount.Currency.Code));
        }

        return transfers;
    }
}
