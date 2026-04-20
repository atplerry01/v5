using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public readonly record struct MediaFileId
{
    public Guid Value { get; }

    public MediaFileId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaFileId cannot be empty.");
        Value = value;
    }
}
