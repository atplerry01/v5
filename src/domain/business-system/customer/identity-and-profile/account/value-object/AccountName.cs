namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public readonly record struct AccountName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public AccountName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AccountName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"AccountName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
