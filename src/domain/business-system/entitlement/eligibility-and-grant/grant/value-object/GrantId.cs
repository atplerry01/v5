using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantId
{
    public Guid Value { get; }

    public GrantId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "GrantId cannot be empty.");
        Value = value;
    }
}
