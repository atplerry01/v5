using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

/// Reference to a live-streaming/archive aggregate id; bare Guid per domain.guard.md rule 13.
public readonly record struct ArchiveRef
{
    public Guid Value { get; }

    public ArchiveRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ArchiveRef cannot be empty.");
        Value = value;
    }
}
