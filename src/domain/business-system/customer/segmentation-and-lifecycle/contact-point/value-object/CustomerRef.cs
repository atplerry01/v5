namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public readonly record struct CustomerRef
{
    public Guid Value { get; }

    public CustomerRef(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerRef value must not be empty.", nameof(value));

        Value = value;
    }
}
