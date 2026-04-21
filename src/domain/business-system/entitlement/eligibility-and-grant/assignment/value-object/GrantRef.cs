using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct GrantRef
{
    public Guid Value { get; }

    public GrantRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "GrantRef cannot be empty.");
        Value = value;
    }
}
