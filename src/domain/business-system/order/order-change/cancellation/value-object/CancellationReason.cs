namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public readonly record struct CancellationReason
{
    public const int MaxLength = 1000;

    public string Value { get; }

    public CancellationReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CancellationReason must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"CancellationReason exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
