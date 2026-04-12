namespace Whycespace.Domain.BusinessSystem.Entitlement.Allocation;

public readonly record struct ResourceId
{
    public Guid Value { get; }

    public ResourceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ResourceId value must not be empty.", nameof(value));
        Value = value;
    }
}
