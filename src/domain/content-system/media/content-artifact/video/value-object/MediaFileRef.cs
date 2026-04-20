using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public readonly record struct MediaFileRef
{
    public Guid Value { get; }

    public MediaFileRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaFileRef cannot be empty.");
        Value = value;
    }
}
