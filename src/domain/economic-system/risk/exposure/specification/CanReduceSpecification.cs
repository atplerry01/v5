using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Risk.Exposure;

public sealed class CanReduceSpecification : Specification<ExposureAggregate>
{
    public override bool IsSatisfiedBy(ExposureAggregate exposure)
    {
        return exposure.Status != ExposureStatus.Closed;
    }
}
