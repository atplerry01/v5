namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct AssignmentId
{
    public Guid Value { get; }

    public AssignmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssignmentId value must not be empty.", nameof(value));

        Value = value;
    }
}
