using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PersistenceAndObservability.Metrics;

public sealed class CanUpdateMetricsSpecification : Specification<MetricsAggregate>
{
    public override bool IsSatisfiedBy(MetricsAggregate entity)
        => entity.Status == MetricsStatus.Capturing || entity.Status == MetricsStatus.Updated;
}
