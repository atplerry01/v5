namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceOption;

public readonly record struct ServiceOptionId
{
    public Guid Value { get; }

    public ServiceOptionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceOptionId value must not be empty.", nameof(value));

        Value = value;
    }
}
