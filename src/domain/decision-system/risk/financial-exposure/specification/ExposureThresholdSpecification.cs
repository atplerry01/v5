using Whycespace.Domain.SharedKernel.Primitive.Money;

namespace Whycespace.Domain.DecisionSystem.Risk.Exposure;

public sealed class ExposureThresholdSpecification
{
    public bool IsSatisfiedBy(Amount currentExposure, Amount threshold)
    {
        return currentExposure.Value <= threshold.Value;
    }
}
