namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public readonly record struct CustomerName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public CustomerName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CustomerName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"CustomerName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
