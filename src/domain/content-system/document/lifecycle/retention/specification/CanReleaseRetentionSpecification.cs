using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Retention;

public sealed class CanReleaseRetentionSpecification : Specification<RetentionAggregate>
{
    public override bool IsSatisfiedBy(RetentionAggregate entity)
        => entity.Status == RetentionStatus.Applied || entity.Status == RetentionStatus.Held;
}
