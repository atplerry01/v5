using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Metadata;

public readonly record struct MetadataSchemaId
{
    public Guid Value { get; }

    public MetadataSchemaId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MetadataSchemaId cannot be empty.");
        Value = value;
    }
}
