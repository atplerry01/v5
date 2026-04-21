using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantSubjectRef
{
    public Guid Value { get; }

    public GrantSubjectRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "GrantSubjectRef cannot be empty.");
        Value = value;
    }
}
