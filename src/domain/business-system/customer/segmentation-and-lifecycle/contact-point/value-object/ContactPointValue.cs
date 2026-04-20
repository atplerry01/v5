namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.ContactPoint;

public readonly record struct ContactPointValue
{
    public const int MaxLength = 512;

    public string Value { get; }

    public ContactPointValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ContactPointValue must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ContactPointValue exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
