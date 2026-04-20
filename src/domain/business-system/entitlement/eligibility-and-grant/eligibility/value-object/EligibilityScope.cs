namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct EligibilityScope
{
    public const int MaxLength = 200;

    public string Value { get; }

    public EligibilityScope(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("EligibilityScope must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"EligibilityScope exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
