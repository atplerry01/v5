namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentCriteria
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public SegmentCriteria(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SegmentCriteria must not be empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"SegmentCriteria exceeds {MaxLength} characters.", nameof(value));

        Value = value;
    }
}
