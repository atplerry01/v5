using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct AssignmentSubjectRef
{
    public Guid Value { get; }

    public AssignmentSubjectRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AssignmentSubjectRef cannot be empty.");
        Value = value;
    }
}
