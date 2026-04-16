using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Lock;

public sealed class CanUnlockSpecification : Specification<LockAggregate>
{
    public override bool IsSatisfiedBy(LockAggregate lockAggregate) =>
        lockAggregate.Status == LockStatus.Locked;
}
