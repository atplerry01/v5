using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

/// Reference to a live-streaming/broadcast aggregate id; bare Guid per domain.guard.md rule 13.
public readonly record struct BroadcastRef
{
    public Guid Value { get; }

    public BroadcastRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "BroadcastRef cannot be empty.");
        Value = value;
    }
}
