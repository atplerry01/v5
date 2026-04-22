using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Moderation;

/// Reference to a stream/broadcast/channel aggregate id; bare Guid per domain.guard.md rule 13.
public readonly record struct ModerationTargetRef
{
    public Guid Value { get; }

    public ModerationTargetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ModerationTargetRef cannot be empty.");
        Value = value;
    }
}
