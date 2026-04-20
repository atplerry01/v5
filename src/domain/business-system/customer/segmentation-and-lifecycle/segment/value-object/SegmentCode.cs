namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Segment;

public readonly record struct SegmentCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public SegmentCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("SegmentCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"SegmentCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
