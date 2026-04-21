using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant;

public readonly record struct GrantScope
{
    public const int MaxLength = 200;

    public string Value { get; }

    public GrantScope(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "GrantScope must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"GrantScope exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
