namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceWindow;

public readonly record struct ServiceWindowId
{
    public Guid Value { get; }

    public ServiceWindowId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceWindowId value must not be empty.", nameof(value));

        Value = value;
    }
}
