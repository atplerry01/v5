namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct AssignmentSubjectRef
{
    public Guid Value { get; }

    public AssignmentSubjectRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssignmentSubjectRef value must not be empty.", nameof(value));

        Value = value;
    }
}
