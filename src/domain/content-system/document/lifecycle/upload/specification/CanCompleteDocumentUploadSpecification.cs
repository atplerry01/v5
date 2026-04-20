using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Lifecycle.Upload;

public sealed class CanCompleteDocumentUploadSpecification : Specification<DocumentUploadAggregate>
{
    public override bool IsSatisfiedBy(DocumentUploadAggregate entity)
        => entity.Status == DocumentUploadStatus.Processing;
}
