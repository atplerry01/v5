namespace Whycespace.Domain.BusinessSystem.Service.ServiceCore.ServiceLevel;

public readonly record struct ServiceLevelId
{
    public Guid Value { get; }

    public ServiceLevelId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ServiceLevelId value must not be empty.", nameof(value));

        Value = value;
    }
}
