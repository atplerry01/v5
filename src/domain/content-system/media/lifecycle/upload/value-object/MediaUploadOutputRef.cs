using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public readonly record struct MediaUploadOutputRef
{
    public Guid Value { get; }

    public MediaUploadOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaUploadOutputRef cannot be empty.");
        Value = value;
    }
}
