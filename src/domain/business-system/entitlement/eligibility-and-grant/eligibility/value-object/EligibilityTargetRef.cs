using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct EligibilityTargetRef
{
    public Guid Value { get; }

    public EligibilityTargetRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EligibilityTargetRef cannot be empty.");
        Value = value;
    }
}
