using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct MediaMetadataId
{
    public Guid Value { get; }

    public MediaMetadataId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaMetadataId cannot be empty.");
        Value = value;
    }
}
