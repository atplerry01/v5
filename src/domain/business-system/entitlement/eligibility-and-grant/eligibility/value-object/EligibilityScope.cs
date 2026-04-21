using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct EligibilityScope
{
    public const int MaxLength = 200;

    public string Value { get; }

    public EligibilityScope(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "EligibilityScope must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"EligibilityScope exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
