using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public sealed class CanUpdateObservabilitySpecification : Specification<ObservabilityAggregate>
{
    public override bool IsSatisfiedBy(ObservabilityAggregate entity)
        => entity.Status == ObservabilityStatus.Capturing || entity.Status == ObservabilityStatus.Updated;
}
