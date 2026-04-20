namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public readonly record struct AssignmentScope
{
    public const int MaxLength = 200;

    public string Value { get; }

    public AssignmentScope(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AssignmentScope must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"AssignmentScope exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
