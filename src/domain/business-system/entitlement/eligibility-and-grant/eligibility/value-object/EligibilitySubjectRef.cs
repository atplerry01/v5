using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct EligibilitySubjectRef
{
    public Guid Value { get; }

    public EligibilitySubjectRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "EligibilitySubjectRef cannot be empty.");
        Value = value;
    }
}
