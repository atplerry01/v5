using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public readonly record struct MediaAssetRef
{
    public Guid Value { get; }

    public MediaAssetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaAssetRef cannot be empty.");
        Value = value;
    }
}
