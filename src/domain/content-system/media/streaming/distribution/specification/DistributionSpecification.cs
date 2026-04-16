using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Streaming.Distribution;

public sealed class DistributionSpecification : Specification<DistributionStatus>
{
    public override bool IsSatisfiedBy(DistributionStatus entity) => entity == DistributionStatus.Active;

    public void EnsureActive(DistributionStatus status)
    {
        if (status == DistributionStatus.Deactivated)
            throw DistributionErrors.CannotMutateDeactivated();
    }
}
