using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Lifecycle.Processing;

public readonly record struct MediaProcessingJobId
{
    public Guid Value { get; }

    public MediaProcessingJobId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "MediaProcessingJobId cannot be empty.");
        Value = value;
    }
}
