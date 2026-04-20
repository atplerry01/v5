using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Version;

public sealed class CanSupersedeSpecification : Specification<DocumentVersionAggregate>
{
    public override bool IsSatisfiedBy(DocumentVersionAggregate entity)
        => entity.Status == VersionStatus.Active;
}
