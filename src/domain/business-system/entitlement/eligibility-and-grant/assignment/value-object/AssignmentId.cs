using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct AssignmentId
{
    public Guid Value { get; }

    public AssignmentId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AssignmentId cannot be empty.");
        Value = value;
    }
}
