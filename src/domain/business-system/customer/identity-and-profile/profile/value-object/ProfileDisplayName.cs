namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public readonly record struct ProfileDisplayName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ProfileDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProfileDisplayName must not be empty.", nameof(value));

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
            throw new ArgumentException($"ProfileDisplayName exceeds {MaxLength} characters.", nameof(value));

        Value = trimmed;
    }
}
