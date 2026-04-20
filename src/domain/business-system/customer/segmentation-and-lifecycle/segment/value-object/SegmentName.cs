namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public SegmentName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SegmentName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"SegmentName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
