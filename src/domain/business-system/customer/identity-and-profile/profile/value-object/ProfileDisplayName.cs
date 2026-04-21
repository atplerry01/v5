using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Profile;

public readonly record struct ProfileDisplayName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public ProfileDisplayName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "ProfileDisplayName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"ProfileDisplayName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
