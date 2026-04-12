namespace Whycespace.Domain.BusinessSystem.Logistic.Tracking;

public readonly record struct TrackingPoint
{
    public string Value { get; }

    public TrackingPoint(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TrackingPoint must not be empty.", nameof(value));

        Value = value;
    }
}
