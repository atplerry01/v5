using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed class CanActivateTemplateSpecification : Specification<DocumentTemplateAggregate>
{
    public override bool IsSatisfiedBy(DocumentTemplateAggregate entity)
        => entity.Status == TemplateStatus.Draft;
}
