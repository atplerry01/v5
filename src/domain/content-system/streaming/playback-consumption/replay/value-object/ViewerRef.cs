using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Replay;

/// Reference to the viewer identity; bare Guid per domain.guard.md rule 13.
public readonly record struct ViewerRef
{
    public Guid Value { get; }

    public ViewerRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ViewerRef cannot be empty.");
        Value = value;
    }
}
