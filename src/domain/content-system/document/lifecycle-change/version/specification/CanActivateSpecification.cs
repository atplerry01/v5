using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.LifecycleChange.Version;

public sealed class CanActivateSpecification : Specification<DocumentVersionAggregate>
{
    public override bool IsSatisfiedBy(DocumentVersionAggregate entity)
        => entity.Status == VersionStatus.Draft;
}
