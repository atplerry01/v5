namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public readonly record struct PolicyBindingId
{
    public Guid Value { get; }

    public PolicyBindingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PolicyBindingId value must not be empty.", nameof(value));

        Value = value;
    }
}
