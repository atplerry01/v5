using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed class CanAllocateToSliceSpecification : Specification<VaultAggregate>
{
    private readonly SliceId _sliceId;
    private readonly Amount _amount;

    public CanAllocateToSliceSpecification(SliceId sliceId, Amount amount)
    {
        _sliceId = sliceId;
        _amount = amount;
    }

    public override bool IsSatisfiedBy(VaultAggregate vault)
    {
        if (_amount.Value <= 0m) return false;

        var slice = vault.Slices.FirstOrDefault(s => s.Id == _sliceId);
        if (slice is null) return false;
        if (slice.Status == SliceStatus.Closed) return false;

        return _amount.Value <= slice.AvailableAmount.Value;
    }
}
