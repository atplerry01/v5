using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public readonly record struct CustomerName
{
    public const int MaxLength = 200;

    public string Value { get; }

    public CustomerName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CustomerName must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"CustomerName exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
