using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed class CanModifyMetadataSpecification : Specification<DocumentMetadataAggregate>
{
    public override bool IsSatisfiedBy(DocumentMetadataAggregate entity)
        => entity.Status == MetadataStatus.Open;
}
