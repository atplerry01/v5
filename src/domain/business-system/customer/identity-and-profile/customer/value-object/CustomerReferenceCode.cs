namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public readonly record struct CustomerReferenceCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public CustomerReferenceCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("CustomerReferenceCode must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"CustomerReferenceCode exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
