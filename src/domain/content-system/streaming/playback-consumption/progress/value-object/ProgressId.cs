using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Progress;

public readonly record struct ProgressId
{
    public Guid Value { get; }

    public ProgressId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ProgressId cannot be empty.");
        Value = value;
    }
}
