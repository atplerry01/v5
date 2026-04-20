namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public readonly record struct ProfileDescriptor
{
    public const int KeyMaxLength = 64;
    public const int ValueMaxLength = 512;

    public string Key { get; }
    public string Value { get; }

    public ProfileDescriptor(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("ProfileDescriptor key must not be empty.", nameof(key));

        if (key.Trim().Length > KeyMaxLength)
            throw new ArgumentException($"ProfileDescriptor key exceeds {KeyMaxLength} characters.", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (value.Length > ValueMaxLength)
            throw new ArgumentException($"ProfileDescriptor value exceeds {ValueMaxLength} characters.", nameof(value));

        Key = key.Trim();
        Value = value;
    }
}
