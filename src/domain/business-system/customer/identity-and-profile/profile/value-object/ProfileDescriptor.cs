using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public readonly record struct ProfileDescriptor
{
    public const int KeyMaxLength = 64;
    public const int ValueMaxLength = 512;

    public string Key { get; }
    public string Value { get; }

    public ProfileDescriptor(string key, string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(key), "ProfileDescriptor key must not be empty.");
        Guard.Against(key.Trim().Length > KeyMaxLength, $"ProfileDescriptor key exceeds {KeyMaxLength} characters.");
        Guard.Against(value is null, "ProfileDescriptor value must not be null.");
        Guard.Against(value!.Length > ValueMaxLength, $"ProfileDescriptor value exceeds {ValueMaxLength} characters.");

        Key = key.Trim();
        Value = value;
    }
}
