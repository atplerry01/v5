using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Customer.IdentityAndProfile.Customer;

public readonly record struct CustomerReferenceCode
{
    public const int MaxLength = 64;

    public string Value { get; }

    public CustomerReferenceCode(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CustomerReferenceCode must not be empty.");

        var trimmed = value.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"CustomerReferenceCode exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
