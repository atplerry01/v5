using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

public readonly record struct ReplayId
{
    public Guid Value { get; }

    public ReplayId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReplayId cannot be empty.");
        Value = value;
    }
}
