using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public readonly record struct DocumentMetadataId
{
    public Guid Value { get; }

    public DocumentMetadataId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentMetadataId cannot be empty.");
        Value = value;
    }
}
