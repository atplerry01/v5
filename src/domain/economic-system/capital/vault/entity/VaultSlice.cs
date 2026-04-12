using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class VaultSlice : Entity
{
    public SliceId Id { get; private set; }
    public VaultId VaultId { get; private set; }
    public Amount TotalCapacity { get; private set; }
    public Amount UsedAmount { get; private set; }
    public Amount AvailableAmount { get; private set; }
    public Currency Currency { get; private set; }
    public SliceStatus Status { get; private set; }

    private VaultSlice() { }

    internal static VaultSlice Create(
        SliceId id,
        VaultId vaultId,
        Amount totalCapacity,
        Currency currency)
    {
        return new VaultSlice
        {
            Id = id,
            VaultId = vaultId,
            TotalCapacity = totalCapacity,
            UsedAmount = new Amount(0m),
            AvailableAmount = totalCapacity,
            Currency = currency,
            Status = SliceStatus.Active
        };
    }

    internal void Deposit(Amount amount)
    {
        TotalCapacity = new Amount(TotalCapacity.Value + amount.Value);
        AvailableAmount = new Amount(AvailableAmount.Value + amount.Value);
    }

    internal void Allocate(Amount amount)
    {
        AvailableAmount = new Amount(AvailableAmount.Value - amount.Value);
        UsedAmount = new Amount(UsedAmount.Value + amount.Value);

        if (AvailableAmount.Value == 0m)
            Status = SliceStatus.FullyAllocated;
    }

    internal void Release(Amount amount)
    {
        UsedAmount = new Amount(UsedAmount.Value - amount.Value);
        AvailableAmount = new Amount(AvailableAmount.Value + amount.Value);

        if (Status == SliceStatus.FullyAllocated)
            Status = SliceStatus.Active;
    }

    internal void Withdraw(Amount amount)
    {
        AvailableAmount = new Amount(AvailableAmount.Value - amount.Value);
        TotalCapacity = new Amount(TotalCapacity.Value - amount.Value);
    }

    internal void EnsureSliceInvariants()
    {
        if (UsedAmount.Value + AvailableAmount.Value != TotalCapacity.Value)
            throw VaultErrors.SliceCapacityInvariantViolation(Id, TotalCapacity, UsedAmount, AvailableAmount);

        if (AvailableAmount.Value < 0m)
            throw VaultErrors.NegativeSliceAvailable(Id);

        if (UsedAmount.Value < 0m)
            throw VaultErrors.NegativeSliceUsed(Id);
    }
}
