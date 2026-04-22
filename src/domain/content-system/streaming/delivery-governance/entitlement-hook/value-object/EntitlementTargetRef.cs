using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

/// Reference to a stream/channel/archive aggregate id; bare Guid per domain.guard.md rule 13.
public readonly record struct EntitlementTargetRef
{
    public Guid Value { get; }

    public EntitlementTargetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EntitlementTargetRef cannot be empty.");
        Value = value;
    }
}
