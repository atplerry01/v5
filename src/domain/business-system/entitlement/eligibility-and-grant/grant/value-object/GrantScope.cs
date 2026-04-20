namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantScope
{
    public const int MaxLength = 200;

    public string Value { get; }

    public GrantScope(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("GrantScope must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"GrantScope exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
