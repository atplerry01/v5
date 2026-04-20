using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Record;

public sealed class CanCloseRecordSpecification : Specification<DocumentRecordAggregate>
{
    public override bool IsSatisfiedBy(DocumentRecordAggregate entity)
        => entity.Status == RecordStatus.Open || entity.Status == RecordStatus.Locked;
}
