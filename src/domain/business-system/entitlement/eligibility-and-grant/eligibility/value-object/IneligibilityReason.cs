using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public readonly record struct IneligibilityReason
{
    public const int MaxLength = 500;

    public string Value { get; }

    public IneligibilityReason(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "IneligibilityReason must not be empty.");

        var trimmed = value!.Trim();
        Guard.Against(trimmed.Length > MaxLength, $"IneligibilityReason exceeds {MaxLength} characters.");

        Value = trimmed;
    }
}
