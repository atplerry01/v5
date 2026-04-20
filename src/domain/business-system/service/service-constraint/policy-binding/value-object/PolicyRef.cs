namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.PolicyBinding;

public readonly record struct PolicyRef
{
    public const int MaxLength = 200;

    public string Value { get; }

    public PolicyRef(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("PolicyRef must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"PolicyRef exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
