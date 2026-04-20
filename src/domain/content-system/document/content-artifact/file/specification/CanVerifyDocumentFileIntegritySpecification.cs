using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public sealed class CanVerifyDocumentFileIntegritySpecification : Specification<DocumentFileAggregate>
{
    public override bool IsSatisfiedBy(DocumentFileAggregate entity)
        => entity.Status == DocumentFileStatus.Registered
           && entity.IntegrityStatus == DocumentFileIntegrityStatus.Unverified;
}
