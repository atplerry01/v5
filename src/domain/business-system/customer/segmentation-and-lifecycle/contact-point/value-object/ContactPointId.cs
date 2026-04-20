namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public readonly record struct ContactPointId
{
    public Guid Value { get; }

    public ContactPointId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ContactPointId value must not be empty.", nameof(value));

        Value = value;
    }
}
