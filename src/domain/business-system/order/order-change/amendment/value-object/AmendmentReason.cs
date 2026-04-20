namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public readonly record struct AmendmentReason
{
    public const int MaxLength = 1000;

    public string Value { get; }

    public AmendmentReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AmendmentReason must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"AmendmentReason exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
