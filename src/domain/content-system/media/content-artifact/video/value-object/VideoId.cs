using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public readonly record struct VideoId
{
    public Guid Value { get; }

    public VideoId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "VideoId cannot be empty.");
        Value = value;
    }
}
