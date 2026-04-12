namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public readonly record struct TrackingId
{
    public Guid Value { get; }

    public TrackingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TrackingId value must not be empty.", nameof(value));

        Value = value;
    }
}
