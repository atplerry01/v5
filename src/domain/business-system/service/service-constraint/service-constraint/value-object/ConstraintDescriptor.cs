namespace Whycespace.Domain.BusinessSystem.Service.ServiceConstraint.ServiceConstraint;

public readonly record struct ConstraintDescriptor
{
    public const int MaxLength = 2000;

    public string Value { get; }

    public ConstraintDescriptor(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ConstraintDescriptor must not be empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"ConstraintDescriptor exceeds {MaxLength} characters.", nameof(value));

        Value = value;
    }
}
