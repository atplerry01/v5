using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Allocation;

public sealed class CanReleaseSpecification : Specification<CapitalAllocationAggregate>
{
    public override bool IsSatisfiedBy(CapitalAllocationAggregate entity) =>
        entity.Status == AllocationStatus.Pending;
}
