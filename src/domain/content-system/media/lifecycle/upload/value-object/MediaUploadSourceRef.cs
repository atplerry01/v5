using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Upload;

/// Reference to the media asset the upload is bound to. Bare-id to avoid
/// cross-BC type imports per domain.guard.md rule 13.
public readonly record struct MediaUploadSourceRef
{
    public Guid Value { get; }

    public MediaUploadSourceRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaUploadSourceRef cannot be empty.");
        Value = value;
    }
}
