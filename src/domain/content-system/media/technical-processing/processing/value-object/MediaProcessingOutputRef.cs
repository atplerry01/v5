using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.TechnicalProcessing.Processing;

public readonly record struct MediaProcessingOutputRef
{
    public Guid Value { get; }

    public MediaProcessingOutputRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaProcessingOutputRef cannot be empty.");
        Value = value;
    }
}
