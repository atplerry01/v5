using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Account;

public readonly record struct AccountName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public AccountName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "AccountName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"AccountName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
