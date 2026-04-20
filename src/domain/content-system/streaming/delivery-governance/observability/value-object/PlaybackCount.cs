using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Observability;

public readonly record struct PlaybackCount
{
    public long Value { get; }

    public PlaybackCount(long value)
    {
        Guard.Against(value < 0, "PlaybackCount cannot be negative.");
        Value = value;
    }
}
