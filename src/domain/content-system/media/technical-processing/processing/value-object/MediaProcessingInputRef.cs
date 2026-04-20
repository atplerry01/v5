using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

/// Reference to the input media artifact (asset/media-file/version) that a
/// media processing job operates on. Bare-id to avoid cross-BC type imports
/// per domain.guard.md rule 13.
public readonly record struct MediaProcessingInputRef
{
    public Guid Value { get; }

    public MediaProcessingInputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaProcessingInputRef cannot be empty.");
        Value = value;
    }
}
