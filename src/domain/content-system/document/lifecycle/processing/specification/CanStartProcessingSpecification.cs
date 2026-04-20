using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Processing;

public sealed class CanStartProcessingSpecification : Specification<DocumentProcessingAggregate>
{
    public override bool IsSatisfiedBy(DocumentProcessingAggregate entity)
        => entity.Status == ProcessingStatus.Requested;
}
