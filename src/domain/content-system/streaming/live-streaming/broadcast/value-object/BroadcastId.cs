using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

public readonly record struct BroadcastId
{
    public Guid Value { get; }

    public BroadcastId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BroadcastId cannot be empty.");
        Value = value;
    }
}
