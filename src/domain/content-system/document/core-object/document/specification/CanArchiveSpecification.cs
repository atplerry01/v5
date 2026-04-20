using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed class CanArchiveSpecification : Specification<DocumentAggregate>
{
    public override bool IsSatisfiedBy(DocumentAggregate entity)
        => entity.Status != DocumentStatus.Archived;
}
