using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

public readonly record struct MediaUploadInputRef
{
    public Guid Value { get; }

    public MediaUploadInputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaUploadInputRef cannot be empty.");
        Value = value;
    }
}
