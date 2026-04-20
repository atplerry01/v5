using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Broadcast;

/// Reference to the parent streaming/stream-core/stream aggregate id, carried
/// as a bare id to avoid cross-BC type imports per domain.guard.md rule 13.
public readonly record struct StreamRef
{
    public Guid Value { get; }

    public StreamRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "StreamRef cannot be empty.");
        Value = value;
    }
}
