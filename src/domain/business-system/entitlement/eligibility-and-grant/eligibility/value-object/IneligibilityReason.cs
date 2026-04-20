namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct IneligibilityReason
{
    public const int MaxLength = 500;

    public string Value { get; }

    public IneligibilityReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("IneligibilityReason must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"IneligibilityReason exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
