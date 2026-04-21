using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantTargetRef
{
    public Guid Value { get; }

    public GrantTargetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "GrantTargetRef cannot be empty.");
        Value = value;
    }
}
