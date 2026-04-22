using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public sealed class CanRefreshEntitlementSpecification : Specification<EntitlementHookAggregate>
{
    public override bool IsSatisfiedBy(EntitlementHookAggregate entity)
        => entity.Status != EntitlementStatus.Unknown && entity.Status != EntitlementStatus.Error;
}
