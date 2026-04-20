namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentId
{
    public Guid Value { get; }

    public SegmentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("SegmentId value must not be empty.", nameof(value));

        Value = value;
    }
}
