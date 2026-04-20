using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public readonly record struct MediaUploadId
{
    public Guid Value { get; }

    public MediaUploadId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaUploadId cannot be empty.");
        Value = value;
    }
}
