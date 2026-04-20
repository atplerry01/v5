using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Bundle;

public sealed class CanModifyBundleSpecification : Specification<DocumentBundleAggregate>
{
    public override bool IsSatisfiedBy(DocumentBundleAggregate entity)
        => entity.Status == BundleStatus.Open;
}
