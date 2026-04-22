using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

public readonly record struct EntitlementHookId
{
    public Guid Value { get; }

    public EntitlementHookId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EntitlementHookId cannot be empty.");
        Value = value;
    }
}
