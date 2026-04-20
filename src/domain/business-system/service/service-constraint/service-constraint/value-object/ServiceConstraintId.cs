namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public readonly record struct ServiceConstraintId
{
    public Guid Value { get; }

    public ServiceConstraintId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceConstraintId value must not be empty.", nameof(value));

        Value = value;
    }
}
