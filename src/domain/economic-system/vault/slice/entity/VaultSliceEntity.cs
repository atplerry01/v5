using Whycespace.Domain.EconomicSystem.Vault.Metrics;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Slice;

/// <summary>
/// A single slice within a vault account. Entity, not aggregate —
/// lifecycle is owned exclusively by VaultAccountAggregate. No slice-level
/// domain events are emitted (per Phase 2B doctrine); state changes ride
/// on the business-level vault events.
///
/// Each slice holds a VaultMetrics reference describing the capital
/// attributed to it. Only Slice1 ever carries Free capital; Slice2 holds
/// Invested capital.
/// </summary>
public sealed class VaultSliceEntity : Entity
{
    public SliceType SliceType { get; private set; }
    public VaultMetrics Metrics { get; private set; }

    private VaultSliceEntity() { }

    internal static VaultSliceEntity Create(SliceType sliceType)
    {
        return new VaultSliceEntity
        {
            SliceType = sliceType,
            Metrics = VaultMetrics.Zero()
        };
    }

    internal void Credit(Amount amount)
    {
        Metrics = Metrics.WithFunding(amount);
    }

    internal void MoveToInvested(Amount amount)
    {
        Metrics = Metrics.WithInvestment(amount);
    }

    internal void Debit(Amount amount)
    {
        Metrics = Metrics.WithDebit(amount);
    }
}
