using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed class CanActivateSpecification : Specification<DocumentVersionAggregate>
{
    public override bool IsSatisfiedBy(DocumentVersionAggregate entity)
        => entity.Status == VersionStatus.Draft;
}
