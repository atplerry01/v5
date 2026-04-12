namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public readonly record struct CapacityId
{
    public Guid Value { get; }

    public CapacityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CapacityId value must not be empty.", nameof(value));

        Value = value;
    }
}
